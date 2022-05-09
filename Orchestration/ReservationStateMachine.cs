using MassTransit;
using Models.Reservations;
using Models.Transport;
using Models.Hotels;
using Models.Payments;

namespace Reservation.Orchestration
{
    public class ReservationStateMachine : MassTransitStateMachine<StatefulReservation>
    {
        public State TemporarilyReserved { get; set; }
        public State SuccessfullyBooked { get; set; }
        public State AwaitingHotelAndTransportReservation { get; set; }
        public State AwaitingHotelReservation { get; set; }
        public State AwaitingTransportReservation { get; set; }
        public State ReservationFailed { get; set; }
        public Event<ReserveOfferEvent> ReserveOfferEvent { get; set; }
        public Event<ReserveTravelReplyEvent> ReserveTravelReplyEvent { get; set; }
        public Event<ReserveRoomsEventReply> ReserveRoomsEventReply { get; set; }
        public Event<PaymentInformationForReservationEvent> PaymentInformationForReservationEvent { get; set; }
        public Event<ProcessPaymentReplyEvent> ProcessPaymentReplyEvent { get; set; }
        public Event<AskForReservationStatusEvent> AskForReservationStatusEvent { get; set; }
        public Schedule<StatefulReservation, ReservationTimeoutEvent> ReservationTimeoutEvent { get; set; }

        public ReservationStateMachine()
        {
            InstanceState(x => x.CurrentState, TemporarilyReserved, AwaitingHotelAndTransportReservation, AwaitingHotelReservation, AwaitingTransportReservation, ReservationFailed, SuccessfullyBooked);
            Event(() => ReserveOfferEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); x.SelectId(context => context.Message.CorrelationId); });
            Event(() => ReserveTravelReplyEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Event(() => ReserveRoomsEventReply, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Event(() => PaymentInformationForReservationEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Event(() => ProcessPaymentReplyEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Event(() => AskForReservationStatusEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Schedule(() => ReservationTimeoutEvent, instance => instance.ReservationTimeoutEventId, s =>
            {
                s.Delay = TimeSpan.FromSeconds(60);
                s.Received = r => r.CorrelateById(context => context.Message.CorrelationId);
            });

            Initially(
                When(ReserveOfferEvent)
                    .Then(async context =>
                    {
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ReserveOfferEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with offer to reserve");
                        }
                        context.Saga.OfferId = payload.Message.OfferId;
                        context.Saga.ReservationId = payload.Message.ReservationId;
                        context.Saga.NumberOfPeople = payload.Message.NumberOfPeople;
                        context.Saga.HotelId = payload.Message.HotelId;
                        context.Saga.HotelName = payload.Message.HotelName;
                        context.Saga.TransportId = payload.Message.TransportId;
                        context.Saga.BeginDate = payload.Message.BeginDate;
                        context.Saga.EndDate = payload.Message.EndDate;
                        context.Saga.Destination = payload.Message.Destination;
                        context.Saga.Departure = payload.Message.Departure;
                        context.Saga.DepartureTime = payload.Message.DepartureTime;
                        context.Saga.Adults = payload.Message.Adults;
                        context.Saga.ChildrenUnder3 = payload.Message.ChildrenUnder3;
                        context.Saga.ChildrenUnder10 = payload.Message.ChildrenUnder10;
                        context.Saga.ChildrenUnder18 = payload.Message.ChildrenUnder18;
                        context.Saga.SmallRooms = payload.Message.SmallRooms;
                        context.Saga.BigRooms = payload.Message.BigRooms;
                        context.Saga.HasInternet = payload.Message.HasInternet;
                        context.Saga.HasBreakfast = payload.Message.HasBreakfast;
                        context.Saga.HasOwnTransport = payload.Message.HasOwnTransport;
                        context.Saga.TravelReservationSuccesful = false;
                        context.Saga.HotelReservationSuccesful = false;
                        context.Saga.PaymentInformationReceived = false;
                        context.Saga.PaymentSuccesful = false;
                    })
                    .If(context => context.Saga.HasOwnTransport,
                        context => context.Publish(context => context.Init<ReserveTravelEvent>(
                                new ReserveTravelEvent(tId: context.Saga.TransportId, seats: context.Saga.NumberOfPeople, uId: context.Saga.UserId)
                                { 
                                    CorrelationId = context.Saga.CorrelationId 
                                })))
                    .Publish(context => context.Init<ReserveRoomsEvent>(
                        new ReserveRoomsEvent(hotelId: context.Saga.HotelId, beginDate: context.Saga.BeginDate,
                            endDate: context.Saga.EndDate, appartmentsAmount: context.Saga.BigRooms, casualRoomAmount: context.Saga.BigRooms, 
                            userId: context.Saga.UserId, reservationNumber: context.Saga.ReservationId)
                        { 
                            CorrelationId = context.Saga.CorrelationId
                        }))
                    .TransitionTo(AwaitingHotelAndTransportReservation));

            During(AwaitingHotelAndTransportReservation,
                When(ReserveRoomsEventReply)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ReserveRoomsEventReply> payload))
                        {
                            throw new Exception("Unable to retrieve payload with hotels response");
                        }
                        context.Saga.HotelReservationSuccesful = payload.Message.Answer == Models.Hotels.ReserveRoomsEventReply.State.RESERVED;
                    })
                    .TransitionTo(AwaitingTransportReservation),
                When(ReserveTravelReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ReserveTravelReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with transport response");
                        }
                        context.Saga.TravelReservationSuccesful = payload.Message.Answer == Models.Transport.ReserveTravelReplyEvent.State.RESERVED;
                    })
                    .TransitionTo(AwaitingHotelReservation),
                When(AskForReservationStatusEvent)
                    .Respond(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent() 
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId, 
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.WAITING_FOR_RESERVATION
                        })));

            During(AwaitingHotelReservation,
                  When(ReserveTravelReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ReserveTravelReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with transport response");
                        }
                        context.Saga.TravelReservationSuccesful = payload.Message.Answer == Models.Transport.ReserveTravelReplyEvent.State.RESERVED;
                    })
                    .TransitionTo(TemporarilyReserved),
                  When(AskForReservationStatusEvent)
                    .Respond(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.WAITING_FOR_RESERVATION
                        })));

            During(AwaitingTransportReservation,
                 When(ReserveRoomsEventReply)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ReserveRoomsEventReply> payload))
                        {
                            throw new Exception("Unable to retrieve payload with hotels response");
                        }
                        context.Saga.HotelReservationSuccesful = payload.Message.Answer == Models.Hotels.ReserveRoomsEventReply.State.RESERVED;
                    })
                    .TransitionTo(TemporarilyReserved),
                 When(AskForReservationStatusEvent)
                    .Respond(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.WAITING_FOR_RESERVATION
                        })));

            WhenEnter(TemporarilyReserved, binder => binder
                .IfElse(context => context.Saga.HotelReservationSuccesful && context.Saga.TravelReservationSuccesful,
                    context => context
                        .Schedule(ReservationTimeoutEvent, context => context.Init<ReservationTimeoutEvent>(
                            new ReservationTimeoutEvent() 
                            {
                                CorrelationId = context.Saga.CorrelationId
                            }))
                        .TransitionTo(TemporarilyReserved),
                    context => context
                        .Publish(context => context.Init<UnreserveRoomsEvent>(
                            new UnreserveRoomsEvent(reservationNumber: context.Saga.ReservationId) 
                            {
                                CorrelationId = context.Saga.CorrelationId
                            }))
                        .Publish(context => context.Init<UnreserveTravelEvent>(
                            new UnreserveTravelEvent(tId: context.Saga.TransportId, uId: context.Saga.UserId, seats: context.Saga.NumberOfPeople) 
                            { 
                                CorrelationId = context.Saga.CorrelationId
                            }))
                        .TransitionTo(ReservationFailed)));

            During(TemporarilyReserved,
                When(ProcessPaymentReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ProcessPaymentReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with hotels response");
                        }
                        context.Saga.PaymentSuccesful = payload.Message.Response == Models.Payments.ProcessPaymentReplyEvent.State.ACCEPTED;
                    })
                    .Unschedule(ReservationTimeoutEvent)
                    .TransitionTo(SuccessfullyBooked),
               When(PaymentInformationForReservationEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, PaymentInformationForReservationEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with transport response");
                        }
                        context.Saga.PaymentInformationReceived = true;
                        context.Saga.Price = payload.Message.Price;
                        context.Saga.CardCredentials = payload.Message.Card;
                    })
                    .Publish(context => context.Init<ProcessPaymentEvent>(
                        new ProcessPaymentEvent(card: context.Saga.CardCredentials, price: context.Saga.Price) 
                        { 
                            CorrelationId = context.Saga.CorrelationId
                        })),
                When(AskForReservationStatusEvent)
                    .Respond(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.WAITING_FOR_PAYMENT,
                            Price = context.Saga.Price
                        })),
                When(ReservationTimeoutEvent.Received)
                    .Unschedule(ReservationTimeoutEvent)
                    .TransitionTo(ReservationFailed));

            During(SuccessfullyBooked,
                When(AskForReservationStatusEvent)
                    .Respond(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.SUCCESFUL,
                            Price = context.Saga.Price
                        }))
                    .Finalize());

            During(ReservationFailed,
                When(AskForReservationStatusEvent)
                    .Respond(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.FAILED,
                            Price = context.Saga.Price
                        })));
        }
    }
}
