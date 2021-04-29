using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainScheduleModel
{
    public enum TrainType { Passenger, Mixed, Weight}

    class Program
    {
        private const int ModelStepCountMax = 100;
        private const int TrainTypeCount = 3;

        //  Время разгрузки (для грузового случайное - указан максимум)
        private const int UnloadTimePassenger = 2;
        private const int UnloadTimeMixed = 3;
        private const int UnloadTimeHeavyMax = 5;

        private static Random _rand = new Random();

        static void Main(string[] args)
        {
            int[] AveTrainCount = new int[TrainTypeCount];
            int[] RailwayCount = new int[TrainTypeCount];

            try
            {
                Console.WriteLine("Введите значения среднего количества прибытых поездов в формате: [легковых] [грузовых] [смешанных]");

                var argarr = Console.ReadLine().Split(' ');

                for (int i = 0; i < TrainTypeCount; i++)
                {
                    AveTrainCount[i] = int.Parse(argarr[i]);
                }

                Console.WriteLine("Введите значения количества путей в том же формате");

                argarr = Console.ReadLine().Split(' ');

                for (int i = 0; i < TrainTypeCount; i++)
                {
                    RailwayCount[i] = int.Parse(argarr[i]);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Неправильно введены параметры.");
                return;
            }

            //  Функция распределения Пуассона (poissons[i].Next() = число тактов до следующего прибытия)
            var poissons = new Poisson[TrainTypeCount];

            for (int i = 0; i < TrainTypeCount; i++)
            {
                poissons[i] = new Poisson(AveTrainCount[i]);
            }

            //  Расписания поездов
            var tSchedule = new List<int>[TrainTypeCount];

            for (int i = 0; i < TrainTypeCount; i++)
            {
                tSchedule[i] = new List<int>();

                var t = 0;

                while (true)
                {
                    t += poissons[i].Next();

                    if (t >= ModelStepCountMax)
                    {
                        break;
                    }

                    else
                    {
                        tSchedule[i].Add(t);
                    }
                }
            }

            //  Динамическое состояние веток платформ
            var tQueues = new List<ArrivalInfo>[TrainTypeCount];

            for (int i = 0; i < TrainTypeCount; i++)
            {
                tQueues[i] = new List<ArrivalInfo>();
            }

            var trainId = 0;

            for (int step = 0; step < ModelStepCountMax; step++)
            {
                for (int i = 0; i < TrainTypeCount; i++)
                {
                    var tType = (TrainType)i;

                    //  Освобождаем разгруженные поезда
                    tQueues[i].RemoveAll(x => x.State == ArrivalInfo.Status.Done);

                    //  Ветки текущей платформы
                    var freeRailways = new List<int>();

                    for (int rw = 0; rw < RailwayCount[i]; rw++)
                    {
                        freeRailways.Add(rw);
                    }

                    foreach (var train in tQueues[i])
                    {
                        if (train.State == ArrivalInfo.Status.Unloading)
                        {
                            freeRailways.Remove(train.RailwayNumber);

                            if (train.Finish == step)
                            {
                                train.State = ArrivalInfo.Status.Done;
                            }
                        }

                        else if (train.State == ArrivalInfo.Status.Waiting)
                        {
                            if (freeRailways.Count > 0)
                            {
                                train.State = ArrivalInfo.Status.Unloading;
                                train.RailwayNumber = freeRailways[0];
                                freeRailways.RemoveAt(0);

                                switch (tType) 
                                {
                                    case TrainType.Passenger:
                                        train.Finish = step + UnloadTimePassenger;
                                        break;

                                    case TrainType.Mixed:
                                        train.Finish = step + UnloadTimeMixed;
                                        break;

                                    case TrainType.Weight:
                                        train.Finish = step + _rand.Next(1, UnloadTimeHeavyMax + 1);
                                        break;
                                }
                            }

                            else
                            {
                                bool foundPlace = false;

                                //  Если поезд легковой или грузовой, ищем место на смешанной платформе
                                if (tType != TrainType.Mixed)
                                {
                                    var allUnloadingTrains = new List<ArrivalInfo>();

                                    //  Легковые поезда, разгружаемые на смешанной платформе
                                    allUnloadingTrains.AddRange(tQueues[(int)TrainType.Passenger].Where(x => x.PlatformType == TrainType.Mixed && x.State == ArrivalInfo.Status.Unloading));

                                    //  Грузовые ...
                                    allUnloadingTrains.AddRange(tQueues[(int)TrainType.Weight].Where(x => x.PlatformType == TrainType.Mixed && x.State == ArrivalInfo.Status.Unloading));

                                    // Смешанные ...
                                    allUnloadingTrains.AddRange(tQueues[(int)TrainType.Mixed].Where(x => x.PlatformType == TrainType.Mixed && x.State == ArrivalInfo.Status.Unloading));

                                    for (int rw = 0; rw < RailwayCount[(int)TrainType.Mixed]; rw++)
                                    {
                                        if (!allUnloadingTrains.Any(x => x.RailwayNumber == rw))
                                        {
                                            //  Нашли место на смешанной платформе
                                            train.State = ArrivalInfo.Status.Unloading;
                                            train.RailwayNumber = rw;

                                            switch (tType)
                                            {
                                                case TrainType.Passenger:
                                                    train.Finish = step + UnloadTimePassenger;
                                                    break;

                                                case TrainType.Mixed:
                                                    train.Finish = step + UnloadTimeMixed;
                                                    break;

                                                case TrainType.Weight:
                                                    train.Finish = step + _rand.Next(1, UnloadTimeHeavyMax + 1);
                                                    break;
                                            }

                                            foundPlace = true;
                                        }
                                    }
                                }

                                //  Очереди допустимы только для грузовых платформ!
                                if (!foundPlace && tType != TrainType.Weight)
                                {
                                    Console.WriteLine($"Поезд {(tType == TrainType.Passenger ? "пассажирского" : "смешанного")} типа встал в очередь, что недопустимо!");
                                    Console.ReadLine();
                                    return;
                                }
                            }
                        }
                    }

                    //  Поезд прибыл по расписанию и начнет разгружаться на следующем шаге
                    if (tSchedule[i].Count() > 0 && tSchedule[i][0] == step)
                    {
                        tQueues[i].Add(new ArrivalInfo()
                        {
                            TrainIdentifier = ++trainId,
                            TrainType = tType,
                            State = ArrivalInfo.Status.Waiting
                        });

                        tSchedule[i].RemoveAt(0);
                    }
                }

                //  Вывод статистики на текущем шаге
                Console.WriteLine($"======================== Шаг {step} ========================");

                for (int i = 0; i < TrainTypeCount; i++)
                {
                    switch ((TrainType)i) 
                    {
                        case TrainType.Passenger:
                            Console.WriteLine($"================== Состояние пассажирских поездов ==================");
                            break;

                        case TrainType.Mixed:
                            Console.WriteLine($"================== Состояние смешанных поездов ==================");
                            break;

                        case TrainType.Weight:
                            Console.WriteLine($"================== Состояние грузовых поездов ==================");
                            break;
                    }

                    foreach (var train in tQueues[i])
                    {
                        Console.WriteLine(train);
                    }

                    Console.WriteLine();
                }
            }

            Console.ReadLine();
        }
    }
}
