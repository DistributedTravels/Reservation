using MassTransit;
using Models.Reservations;
using Models.Transport;
using Models.Hotels;
using Models.Payments;
using Models.Reservations.Dto;

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
        public State ProcessingPayment { get; set; }
        public Event<ReserveOfferEvent> ReserveOfferEvent { get; set; }
        public Event<ReserveTravelReplyEvent> ReserveTravelReplyEvent { get; set; }
        public Event<ReserveRoomsEventReply> ReserveRoomsEventReply { get; set; }
        public Event<PaymentInformationForReservationEvent> PaymentInformationForReservationEvent { get; set; }
        public Event<ProcessPaymentReplyEvent> ProcessPaymentReplyEvent { get; set; }
        public Event<AskForReservationStatusEvent> AskForReservationStatusEvent { get; set; }
        public Schedule<StatefulReservation, ReservationTimeoutEvent> ReservationTimeoutEvent { get; set; }

        public ReservationStateMachine()
        {
            InstanceState(x => x.CurrentState, TemporarilyReserved, AwaitingHotelAndTransportReservation, 
                AwaitingHotelReservation, AwaitingTransportReservation, ReservationFailed, SuccessfullyBooked, ProcessingPayment);
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
                        context.Saga.UserId = payload.Message.UserId;
                        context.Saga.HasPromotionCode = payload.Message.HasPromotionCode;
                    })
                    .RespondAsync(context => context.Init<ReserveOfferReplyEvent>(
                        new ReserveOfferReplyEvent()
                        {
                            Id = context.Saga.CorrelationId,
                            CorrelationId = context.Saga.CorrelationId
                        }))
                    .PublishAsync(context => context.Init<ReserveRoomsEvent>(
                        new ReserveRoomsEvent()
                        {
                            HotelId = context.Saga.HotelId,
                            BeginDate = context.Saga.BeginDate,
                            EndDate = context.Saga.EndDate,
                            AppartmentsAmount = context.Saga.BigRooms,
                            CasualRoomAmount = context.Saga.SmallRooms,
                            UserId = context.Saga.UserId,
                            ReservationNumber = context.Saga.ReservationId,
                            Breakfast = context.Saga.HasBreakfast,
                            Wifi = context.Saga.HasInternet,
                            CorrelationId = context.Saga.CorrelationId
                        }))
                    .IfElse(context => context.Saga.HasOwnTransport == false,
                        context => context
                            .PublishAsync(context => context.Init<ReserveTravelEvent>(
                                new ReserveTravelEvent()
                                {
                                    TravelId = context.Saga.TransportId,
                                    Seats = context.Saga.NumberOfPeople,
                                    ReserveId = context.Saga.ReservationId,
                                    CorrelationId = context.Saga.CorrelationId
                                }))
                            .TransitionTo(AwaitingHotelAndTransportReservation),
                        context => context
                            .Then(context => context.Saga.TravelReservationSuccesful = true)
                            .TransitionTo(AwaitingHotelReservation)));

            WhenEnter(AwaitingHotelAndTransportReservation, binder => binder
                .Then(context =>
                {
                    Console.WriteLine("ENTERED AWAITING HOTELS AND TRANSPORT RESERVATION");
                }));

            During(AwaitingHotelAndTransportReservation,
                When(ReserveRoomsEventReply)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ReserveRoomsEventReply> payload))
                        {
                            throw new Exception("Unable to retrieve payload with hotels response");
                        }
                        context.Saga.HotelPrice = payload.Message.Price;
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
                        context.Saga.TransportPrice = payload.Message.Price;
                        context.Saga.TravelReservationSuccesful = payload.Message.Answer == Models.Transport.ReserveTravelReplyEvent.State.RESERVED;
                    })
                    .TransitionTo(AwaitingHotelReservation),
                When(AskForReservationStatusEvent)
                    .RespondAsync(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent() 
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId, 
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.WAITING_FOR_RESERVATION
                        })));

            WhenEnter(AwaitingTransportReservation, binder => binder
                .Then(context =>
                {
                    Console.WriteLine("ENTERED AWAITING TRANSPORT RESERVATION");
                }));

            During(AwaitingTransportReservation,
                  When(ReserveTravelReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ReserveTravelReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with transport response");
                        }
                        context.Saga.TransportPrice = payload.Message.Price;
                        context.Saga.TravelReservationSuccesful = payload.Message.Answer == Models.Transport.ReserveTravelReplyEvent.State.RESERVED;
                    })
                    .TransitionTo(TemporarilyReserved),
                  When(AskForReservationStatusEvent)
                    .RespondAsync(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.WAITING_FOR_RESERVATION
                        })));

            WhenEnter(AwaitingHotelReservation, binder => binder
                .Then(context =>
                {
                    Console.WriteLine("ENTERED AWAITING HOTEL RESERVATION");
                }));

            During(AwaitingHotelReservation,
                 When(ReserveRoomsEventReply)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ReserveRoomsEventReply> payload))
                        {
                            throw new Exception("Unable to retrieve payload with hotels response");
                        }
                        context.Saga.HotelPrice = payload.Message.Price;
                        context.Saga.HotelReservationSuccesful = payload.Message.Answer == Models.Hotels.ReserveRoomsEventReply.State.RESERVED;
                    })
                    .TransitionTo(TemporarilyReserved),
                 When(AskForReservationStatusEvent)
                    .RespondAsync(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.WAITING_FOR_RESERVATION
                        })));

            WhenEnter(TemporarilyReserved, binder => binder
                .Then(context =>
                {
                    Console.WriteLine("ENTERED TEMPORARILY RESERVED");  
                })
                .IfElse(context => context.Saga.HotelReservationSuccesful && context.Saga.TravelReservationSuccesful,
                    context => context
                        .Schedule(ReservationTimeoutEvent, context => context.Init<ReservationTimeoutEvent>(
                            new ReservationTimeoutEvent() 
                            {
                                CorrelationId = context.Saga.CorrelationId
                            }))
                        .TransitionTo(TemporarilyReserved),
                    context => context
                        .TransitionTo(ReservationFailed)));

            During(TemporarilyReserved,
               When(PaymentInformationForReservationEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, PaymentInformationForReservationEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with transport response");
                        }
                        context.Saga.PaymentInformationReceived = true;
                        context.Saga.Price = 1.5 * (context.Saga.HotelPrice + context.Saga.TransportPrice * context.Saga.NumberOfPeople) * (context.Saga.HasPromotionCode ? 0.9 : 1.0);
                        context.Saga.CardCredentials = payload.Message.Card;
                    })
                    .RespondAsync(context => context.Init<PaymentInformationForReservationReplyEvent>(
                        new PaymentInformationForReservationReplyEvent()
                        {
                            CorrelationId = context.Saga.CorrelationId
                        }))
                    .PublishAsync(context => context.Init<ProcessPaymentEvent>(
                        new ProcessPaymentEvent(card: context.Saga.CardCredentials, price: context.Saga.Price) 
                        { 
                            Card = context.Saga.CardCredentials,
                            Price = context.Saga.Price,
                            CorrelationId = context.Saga.CorrelationId
                        }))
                    .TransitionTo(ProcessingPayment),
                When(AskForReservationStatusEvent)
                    .RespondAsync(context => context.Init<AskForReservationStatusReplyEvent>(
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

            During(ProcessingPayment,
                When(ProcessPaymentReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulReservation, ProcessPaymentReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with hotels response");
                        }
                        context.Saga.PaymentSuccesful = payload.Message.Response == Models.Payments.ProcessPaymentReplyEvent.State.ACCEPTED;
                    })
                    .IfElse(context => context.Saga.PaymentSuccesful,
                        context => context.TransitionTo(SuccessfullyBooked),
                        context => context.TransitionTo(TemporarilyReserved)),
                When(AskForReservationStatusEvent)
                    .RespondAsync(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.PROCESSING_PAYMENT,
                            Price = context.Saga.Price
                        })),
                When(ReservationTimeoutEvent.Received)
                    .Unschedule(ReservationTimeoutEvent)
                    .TransitionTo(ReservationFailed));

            WhenEnter(SuccessfullyBooked, binder => binder
                .Then(context =>
                {
                    Console.WriteLine("ENTERED SUCCESSFULLY BOOKED");
                })
                .PublishAsync(context => context.Init<SaveReservationToDatabaseEvent>(
                    new SaveReservationToDatabaseEvent()
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        Reservation = new ReservationDto()
                        {
                            UserId = context.Saga.UserId,
                            TransportId = context.Saga.TransportId,
                            HotelName = context.Saga.HotelName,
                            HotelId = context.Saga.HotelId,
                            Destination = context.Saga.Destination,
                            Departure = context.Saga.Departure,
                            NumberOfPeople = context.Saga.NumberOfPeople,
                            BeginDate = context.Saga.BeginDate,
                            EndDate = context.Saga.EndDate,
                            DepartureTime = context.Saga.DepartureTime,
                            Status = "BOOKED",
                            Adults = context.Saga.Adults,
                            ChildrenUnder3 = context.Saga.ChildrenUnder3,
                            ChildrenUnder10 = context.Saga.ChildrenUnder10,
                            ChildrenUnder18 = context.Saga.ChildrenUnder18,
                            SmallRooms = context.Saga.SmallRooms,
                            BigRooms = context.Saga.BigRooms,
                            HasInternet = context.Saga.HasInternet,
                            HasBreakfast = context.Saga.HasBreakfast,
                            HasOwnTransport = context.Saga.HasOwnTransport
                        }
                    }))
                .Unschedule(ReservationTimeoutEvent)
                .PublishAsync(context => context.Init<NewReservationSuccessfullyBookedEvent>(
                    new NewReservationSuccessfullyBookedEvent()
                    {
                        Destination = context.Saga.Destination,
                        HotelName = context.Saga.HotelName,
                        User = context.Saga.CardCredentials.FullName
                    })));

            During(SuccessfullyBooked,
                When(AskForReservationStatusEvent)
                    .RespondAsync(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.SUCCESFUL,
                            Price = context.Saga.Price
                        })));

            WhenEnter(ReservationFailed, binder => binder
                .Then(context =>
                {
                    Console.WriteLine("ENTERED RESERVATION FAILED");
                })
                .PublishAsync(context => context.Init<UnreserveRoomsEvent>(
                    new UnreserveRoomsEvent() 
                    {
                        ReservationNumber = context.Saga.ReservationId,
                        CorrelationId = context.Saga.CorrelationId
                    }))
                .PublishAsync(context => context.Init<UnreserveTravelEvent>(
                    new UnreserveTravelEvent() 
                    { 
                        ReserveId = context.Saga.ReservationId,
                        CorrelationId = context.Saga.CorrelationId
                    })));

            During(ReservationFailed,
                When(AskForReservationStatusEvent)
                    .RespondAsync(context => context.Init<AskForReservationStatusReplyEvent>(
                        new AskForReservationStatusReplyEvent()
                        {
                            ReservationId = context.Saga.ReservationId,
                            CorrelationId = context.Saga.CorrelationId,
                            ReservationStatus = AskForReservationStatusReplyEvent.Status.FAILED,
                            Price = context.Saga.Price
                        })),
                When(PaymentInformationForReservationEvent)
                    .RespondAsync(context => context.Init<PaymentInformationForReservationReplyEvent>(
                        new PaymentInformationForReservationReplyEvent()
                        {
                            CorrelationId = context.Saga.CorrelationId
                        })));
        }
    }
}
