using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainScheduleModel
{
    public class ArrivalInfo
    {
        public enum Status { Waiting, Unloading, Done }

        //  Задаются при прибытии поезда
        public int TrainIdentifier { get; set; }
        public TrainType TrainType { get; set; }
        public Status State { get; set; }

        // Задаются после назначения на ветку
        public int RailwayNumber { get; set; }
        public int Finish { get; set; }
        public TrainType PlatformType { get; set; }

        public override string ToString()
        {
            switch (State)
            {
                case Status.Waiting:
                    return $"{GetTrainType()} поезд №{TrainIdentifier} ожидает постановки на платформу";

                case Status.Unloading:
                    return $"{GetTrainType()} поезд №{TrainIdentifier} разгружается на {GetPlatformType()} платформе, ветка №{RailwayNumber + 1}, завершение в {Finish}";

                case Status.Done:
                    return $"{GetTrainType()} поезд №{TrainIdentifier} разгружен на {GetPlatformType()} платформе";
            }

            return null;
        }

        public string GetTrainType()
        {
            switch (TrainType) 
            {
                case TrainType.Passenger:
                    return "Пассажирский";

                case TrainType.Mixed:
                    return "Смешанный";

                case TrainType.Weight:
                    return "Грузовой";
            }

            return null;
        }

        public string GetPlatformType()
        {
            switch (TrainType)
            {
                case TrainType.Passenger:
                    return "пассажирской";

                case TrainType.Mixed:
                    return "смешанной";

                case TrainType.Weight:
                    return "грузовой";
            }

            return null;
        }
    }
}
