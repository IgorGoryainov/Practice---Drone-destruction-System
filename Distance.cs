using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class Distance : MonoBehaviour
{
    public Transform RLS1;

    public Transform RLS2;

    public Transform RLS3;

    public Transform RLS4;

    public Transform RLS5;

    public Transform Drone;

    public float accuracyDistanceActive = 1f;

    public float accuracyAngleActive = 3f;

    public float accuracyDistancePassive = 3f;

    public float accuracyAnglePassive = 3f;

    public double Target_X;

    public double Target_Y;

    public double Target_Z;

    public double Nav_Error;

    public class
    RLS_Active // Класс активной РЛС
    {
        public Transform RLSPos;

        private Transform Target;

        public int angleLimit_1; // Углы ограничивающие зоны сканирования по азимуту

        public int angleLimit_2;

        private float xSide; // Разница между координатой цели и РЛС

        private float zSide;

        private float ySide;

        private float angle;

        private float angleRadians;

        private double elevationAngle; // Угол места, для ограничения обзора

        private float distance;

        private float accuracyDistance;

        private float accuracyAngle;

        public RLS_Active(
            Transform RLS,
            Transform Drones,
            int angle1,
            int angle2,
            float accuracyDistance,
            float accuracyAngle
        )
        {
            this.accuracyDistance = accuracyDistance;
            this.accuracyAngle = accuracyAngle;
            RLSPos = RLS;
            Target = Drones;
            angleLimit_1 = angle1;
            angleLimit_2 = angle2;
            xSide = Target.position.x - RLSPos.position.x;
            zSide = Target.position.z - RLSPos.position.z;
            ySide = Target.position.y - RLSPos.position.y;
            elevationAngle =
                Mathf
                    .Atan2(ySide,
                    Mathf.Sqrt(Mathf.Pow(xSide, 2) + Mathf.Pow(zSide, 2))) *
                Mathf.Rad2Deg;
            angle = Mathf.Atan2(xSide, zSide) * Mathf.Rad2Deg;
            angleRadians = Mathf.Atan2(xSide, zSide); // Тот же вывод угла, на в радианах
            distance = Vector3.Distance(RLSPos.position, Target.position);
        } // Конструктор, сразу считающий расстояние, угол до цели (без учета погрешностей) и проверки на видимость

        public float getAngle()
        {
            if (
                (angle >= angleLimit_1) &
                (angle <= angleLimit_2) &
                (distance >= 20) &
                (distance <= 1500) //Проверка на видимость цели
            )
            {
                return angle + Random.Range(-1 * accuracyAngle, accuracyAngle); //Вывод угла с погрешностью
            }
            else
            {
                return 0;
            }
        }

        public float getAngleRadians()
        {
            if (
                (angle >= angleLimit_1) &
                (angle <= angleLimit_2) &
                (distance >= 20) &
                (distance <= 1500) // Проверка на видимость цели
            )
            {
                return angleRadians +
                Random.Range(-1 * accuracyAngle, accuracyAngle) * Mathf.Deg2Rad; //Вывод угла в радианах, с погрешностью
            }
            else
            {
                return 0;
            }
        }

        public float getDistance()
        {
            if (
                (angle >= angleLimit_1) &
                (angle <= angleLimit_2) &
                (distance >= 20) &
                (distance <= 1500) &
                (Math.Abs(elevationAngle) < 13) //Проверка на видимость цели, дополненная ограничением по углу места, впредыдущих проверках этого не было, т.к. без этого метода, другие не будут вызываться
            )
            {
                return distance +
                Random.Range(-1 * accuracyDistance, accuracyDistance);
            }
            else
            {
                return 0;
            }
        }
    }

    public class
    RLS_Passive //Класс пассивной РЛС
    {
        public Transform RLSPos;

        private Transform Target;

        private float xSide;

        private float zSide;

        private float angle;

        private float angleRadians;

        private float distance;

        private float accuracyDistance;

        private float accuracyAngle;

        public RLS_Passive(
            Transform RLS,
            Transform Drones,
            float accuracyDistance,
            float accuracyAngle
        )
        {
            this.accuracyDistance = accuracyDistance;
            this.accuracyAngle = accuracyAngle;
            RLSPos = RLS;
            Target = Drones;
            xSide = Target.position.x - RLSPos.position.x;
            zSide = Target.position.z - RLSPos.position.z;
            angle = Mathf.Atan2(xSide, zSide) * Mathf.Rad2Deg;
            angleRadians = Mathf.Atan2(xSide, zSide); //Тот же расчет угла, но в радианах
            distance = Vector3.Distance(RLSPos.position, Target.position);
        } // Конструктор, сразу считающий расстояние, угол до цели (без учета погрешностей) и проверки на видимость

        public float getAngle()
        {
            if (
                (distance > 5) & (distance <= 7000) //Проверка на видимость для пассивной РЛС
            )
            {
                return angle + Random.Range(-1 * accuracyAngle, accuracyAngle); //Вывод угла с погрешностью
            }
            else
            {
                return 0;
            }
        }

        public float getAngleRadians()
        {
            if (
                (distance > 5) & (distance <= 7000) //Проверка на видимость для пассивной РЛС
            )
            {
                return angleRadians +
                Random.Range(-1 * accuracyAngle, accuracyAngle) * Mathf.Deg2Rad; //Вывод угла в радианах с погрешностью
            }
            else
            {
                return 0;
            }
        }

        public float getDistance()
        {
            if (
                (distance > 5) & (distance <= 7000) //Проверка на видимость для пассивной РЛС
            )
            {
                return distance +
                Random.Range(-1 * accuracyDistance, accuracyDistance); //Вывод расстояния до цели с погрешностью
            }
            else
            {
                return 0;
            }
        }
    }

    public class
    NavigationSystem //Класс нацигационной системы, ее методы, основываясь на значениях, полученных с РЛС, рассчитывают координаты цели
    {
        struct coords
        {
            public double z;

            public double x;

            public double y; // Высота!!!

            public double y_p; // Высота, рассчитанная, что цель находится выше сеснсора

            public double y_n; // Высота, рассчитанная, что цель находится ниже сенсора
        } // Структура для хранения координат цели

        private RLS_Active

                RLS1_A,
                RLS2_A,
                RLS3_A,
                RLS4_A; // Объекты классов РЛС

        private RLS_Passive RLS5_P; // Объект класса РЛС

        public double tan_1; // Тангенс угла между активной РЛС и целью

        public double tan_2; // Тангенс угла между пассивной РЛС и целью

        private coords targetCoords;

        public NavigationSystem(
            RLS_Active RLS1_A,
            RLS_Active RLS2_A,
            RLS_Active RLS3_A,
            RLS_Active RLS4_A,
            RLS_Passive RLS5_P
        )
        {
            this.RLS1_A = RLS1_A;
            this.RLS2_A = RLS2_A;
            this.RLS3_A = RLS3_A;
            this.RLS4_A = RLS4_A;
            this.RLS5_P = RLS5_P;
            targetCoords = getTargetCoords();
        } // Конструктор, в который передаем все действующие объекты РЛС, не важно видят ли они цель или нет

        public bool
        checkTarget() //Проверка на видимость цели, хотя бы одной из РЛС
        {
            return (
            (RLS5_P.getDistance() != 0) |
            (RLS1_A.getDistance() != 0) |
            (RLS2_A.getDistance() != 0) |
            (RLS3_A.getDistance() != 0) |
            (RLS4_A.getDistance() != 0)
            );
        }

        public bool
        PossibilityGetTargetCoords() // Проверка на возможность определения координат цели, т.е. проверка на видимость цели одной пассивной РЛС и одной из активной РЛС
        {
            return (
            (RLS5_P.getDistance() != 0) &
            (
            (RLS1_A.getDistance() != 0) |
            (RLS2_A.getDistance() != 0) |
            (RLS3_A.getDistance() != 0) |
            (RLS4_A.getDistance() != 0)
            )
            );
        }

        private coords
        getTargetCoords() // Метод по расчету координат цели
        {
            coords target_coords = new coords(); //Создаем объект структуры с координатами
            int iterationCount = 20; //Столько раз запрашиваем углы и дистанцию с РЛС, дабы получить среднее, а после по этому среднему считать координаты
            if (PossibilityGetTargetCoords())
            {
                if (
                    RLS1_A.getDistance() != 0 //Проверка, если цель зафиксирована первой активной РЛС
                )
                {
                    float
                        averageAngleP = 0.0f,
                        averageAngleA = 0.0f,
                        averageDistance = 0.0f; // Сумма полученных углов в кол-ве выше указанных итераций
                    for (int i = 0; i < iterationCount; ++i)
                    {
                        averageAngleP += RLS5_P.getAngleRadians();
                        averageAngleA += RLS1_A.getAngleRadians();
                        averageDistance += RLS1_A.getDistance();
                    }
                    var rls5Angle = averageAngleP / iterationCount; //Получаем средний угол
                    var rls1Angle = averageAngleA / iterationCount; //Получаем средний угол
                    var rls1Distance = averageDistance / iterationCount; //Получаем среднюю дистанцию
                    tan_1 = Mathf.Tan(rls5Angle);
                    tan_2 = Mathf.Tan(rls1Angle);
                    Debug.Log("RLS1"); //Вывод в консоль, какая активная РЛС захватила цель
                    target_coords.z =
                        (
                        -1 * tan_1 / tan_2 * (RLS5_P.RLSPos.position.z) +
                        1 /
                        tan_2 *
                        (RLS5_P.RLSPos.position.x - RLS1_A.RLSPos.position.x) +
                        RLS1_A.RLSPos.position.z
                        ) /
                        (1 - tan_1 / tan_2); // Расчет координат цели
                    target_coords.x =
                        (target_coords.z - RLS1_A.RLSPos.position.z) * tan_2 +
                        RLS1_A.RLSPos.position.x; // Расчет координат цели
                    target_coords.y_p =
                        Math
                            .Sqrt(Math.Pow(rls1Distance, 2) -
                            Math
                                .Pow((
                                target_coords.x - RLS1_A.RLSPos.position.x
                                ),
                                2) -
                            Math
                                .Pow((
                                target_coords.z - RLS1_A.RLSPos.position.z
                                ),
                                2)) +
                        RLS1_A.RLSPos.position.y; // Расчет высоты цели, если цель выше РЛС
                    target_coords.y_n =
                        RLS1_A.RLSPos.position.y -
                        Math
                            .Sqrt(Math.Pow(rls1Distance, 2) -
                            Math
                                .Pow((
                                target_coords.x - RLS1_A.RLSPos.position.x
                                ),
                                2) -
                            Math
                                .Pow((
                                target_coords.z - RLS1_A.RLSPos.position.z
                                ),
                                2)); // Расчет высоты цели, если цели ниже РЛС
                }
                else if (
                    RLS2_A.getDistance() != 0 // Смотреть комментарии для РЛС1
                )
                {
                    float
                        averageAngleP = 0.0f,
                        averageAngleA = 0.0f,
                        averageDistance = 0.0f; // Смотреть комментарии для РЛС1
                    for (int i = 0; i < iterationCount; ++i)
                    {
                        averageAngleP += RLS5_P.getAngleRadians();
                        averageAngleA += RLS2_A.getAngleRadians();
                        averageDistance += RLS2_A.getDistance();
                    }
                    var rls5Angle = averageAngleP / iterationCount;
                    var rls2Angle = averageAngleA / iterationCount;
                    var rls2Distance = averageDistance / iterationCount;
                    tan_1 = Mathf.Tan(rls5Angle); // Смотреть комментарии для РЛС1
                    tan_2 = Mathf.Tan(rls2Angle); // Смотреть комментарии для РЛС1
                    Debug.Log("RLS2"); //Вывод в консоль, какая активная РЛС захватила цель
                    target_coords.z =
                        (
                        -1 * tan_1 / tan_2 * (RLS5_P.RLSPos.position.z) +
                        1 /
                        tan_2 *
                        (RLS5_P.RLSPos.position.x - RLS2_A.RLSPos.position.x) +
                        RLS2_A.RLSPos.position.z
                        ) /
                        (1 - tan_1 / tan_2); // Смотреть комментарии для РЛС1
                    target_coords.x =
                        (target_coords.z - RLS2_A.RLSPos.position.z) * tan_2 +
                        RLS2_A.RLSPos.position.x; // Смотреть комментарии для РЛС1
                    target_coords.y_p =
                        Math
                            .Sqrt(Math.Pow(rls2Distance, 2) -
                            Math
                                .Pow((
                                target_coords.x - RLS2_A.RLSPos.position.x
                                ),
                                2) -
                            Math
                                .Pow((
                                target_coords.z - RLS2_A.RLSPos.position.z
                                ),
                                2)) +
                        RLS2_A.RLSPos.position.y; // Смотреть комментарии для РЛС1
                    target_coords.y_n =
                        RLS2_A.RLSPos.position.y -
                        Math
                            .Sqrt(Math.Pow(rls2Distance, 2) -
                            Math
                                .Pow((
                                target_coords.x - RLS2_A.RLSPos.position.x
                                ),
                                2) -
                            Math
                                .Pow((
                                target_coords.z - RLS2_A.RLSPos.position.z
                                ),
                                2)); // Смотреть комментарии для РЛС1
                }
                else if (
                    RLS3_A.getDistance() != 0 // Смотреть комментарии для РЛС1
                )
                {
                    float
                        averageAngleP = 0.0f,
                        averageAngleA = 0.0f,
                        averageDistance = 0.0f; // Смотреть комментарии для РЛС1
                    for (int i = 0; i < iterationCount; ++i)
                    {
                        averageAngleP += RLS5_P.getAngleRadians();
                        averageAngleA += RLS3_A.getAngleRadians();
                        averageDistance += RLS3_A.getDistance();
                    }
                    var rls5Angle = averageAngleP / iterationCount;
                    var rls3Angle = averageAngleA / iterationCount;
                    var rls3Distance = averageDistance / iterationCount;
                    tan_1 = Mathf.Tan(rls5Angle);
                    tan_2 = Mathf.Tan(rls3Angle);
                    Debug.Log("RLS3"); //Вывод в консоль, какая активная РЛС захватила цель
                    target_coords.z =
                        (
                        -1 * tan_1 / tan_2 * (RLS5_P.RLSPos.position.z) +
                        1 /
                        tan_2 *
                        (RLS5_P.RLSPos.position.x - RLS3_A.RLSPos.position.x) +
                        RLS3_A.RLSPos.position.z
                        ) /
                        (1 - tan_1 / tan_2); // Смотреть комментарии для РЛС1
                    target_coords.x =
                        (target_coords.z - RLS3_A.RLSPos.position.z) * tan_2 +
                        RLS3_A.RLSPos.position.x; // Смотреть комментарии для РЛС1
                    target_coords.y_p =
                        Math
                            .Sqrt(Math.Pow(rls3Distance, 2) -
                            Math
                                .Pow((
                                target_coords.x - RLS3_A.RLSPos.position.x
                                ),
                                2) -
                            Math
                                .Pow((
                                target_coords.z - RLS3_A.RLSPos.position.z
                                ),
                                2)) +
                        RLS3_A.RLSPos.position.y; // Смотреть комментарии для РЛС1
                    target_coords.y_n =
                        RLS3_A.RLSPos.position.y -
                        Math
                            .Sqrt(Math.Pow(rls3Distance, 2) -
                            Math
                                .Pow((
                                target_coords.x - RLS3_A.RLSPos.position.x
                                ),
                                2) -
                            Math
                                .Pow((
                                target_coords.z - RLS3_A.RLSPos.position.z
                                ),
                                2)); // Смотреть комментарии для РЛС1
                } // Если цель поймана РЛС 4
                else
                {
                    float
                        averageAngleP = 0.0f,
                        averageAngleA = 0.0f,
                        averageDistance = 0.0f; // Смотреть комментарии для РЛС1
                    for (int i = 0; i < iterationCount; ++i)
                    {
                        averageAngleP += RLS5_P.getAngleRadians();
                        averageAngleA += RLS4_A.getAngleRadians();
                        averageDistance += RLS4_A.getDistance();
                    }
                    var rls5Angle = averageAngleP / iterationCount;
                    var rls4Angle = averageAngleA / iterationCount;
                    var rls4Distance = averageDistance / iterationCount;
                    tan_1 = Mathf.Tan(rls5Angle); // Смотреть комментарии для РЛС1
                    tan_2 = Mathf.Tan(rls4Angle);
                    Debug.Log("RLS4"); //Вывод в консоль, какая активная РЛС захватила цель
                    target_coords.z =
                        (
                        -1 * tan_1 / tan_2 * (RLS5_P.RLSPos.position.z) +
                        1 /
                        tan_2 *
                        (RLS5_P.RLSPos.position.x - RLS4_A.RLSPos.position.x) +
                        RLS4_A.RLSPos.position.z
                        ) /
                        (1 - tan_1 / tan_2); // Смотреть комментарии для РЛС1
                    target_coords.x =
                        (target_coords.z - RLS4_A.RLSPos.position.z) * tan_2 +
                        RLS4_A.RLSPos.position.x;
                    target_coords.y_p =
                        Math
                            .Sqrt(Math.Pow(rls4Distance, 2) -
                            Math
                                .Pow((
                                target_coords.x - RLS4_A.RLSPos.position.x
                                ),
                                2) -
                            Math
                                .Pow((
                                target_coords.z - RLS4_A.RLSPos.position.z
                                ),
                                2)) +
                        RLS4_A.RLSPos.position.y; // Смотреть комментарии для РЛС1
                    target_coords.y_n =
                        RLS4_A.RLSPos.position.y -
                        Math
                            .Sqrt(Math.Pow(rls4Distance, 2) -
                            Math
                                .Pow((
                                target_coords.x - RLS4_A.RLSPos.position.x
                                ),
                                2) -
                            Math
                                .Pow((
                                target_coords.z - RLS4_A.RLSPos.position.z
                                ),
                                2)); // Смотреть комментарии для РЛС1
                }

                float averageDistanceP = 0.0f; // Расчет пассивной РЛС, сумма дистанций в кол-ве итераций, указанных выше
                for (int i = 0; i < iterationCount; ++i)
                {
                    averageDistanceP += RLS5_P.getDistance();
                }
                var rls5Distance = averageDistanceP / iterationCount; // Находим среднее расстояния до цели

                // Расчет координат x и y уже не производим
                var y_target_2p =
                    Math
                        .Sqrt(Math.Pow(rls5Distance, 2) -
                        Math
                            .Pow((target_coords.x - RLS5_P.RLSPos.position.x),
                            2) -
                        Math
                            .Pow((target_coords.z - RLS5_P.RLSPos.position.z),
                            2)) +
                    RLS5_P.RLSPos.position.y; // Рас
                var y_target_2n =
                    RLS5_P.RLSPos.position.y -
                    Math
                        .Sqrt(Math.Pow(rls5Distance, 2) -
                        Math
                            .Pow((target_coords.x - RLS5_P.RLSPos.position.x),
                            2) -
                        Math
                            .Pow((target_coords.z - RLS5_P.RLSPos.position.z),
                            2));

                target_coords.y = (target_coords.y_p + y_target_2p) / 2;
                //target_coords.y = target_coords.y_p;
            }
            return target_coords;
        }

        public double
        getTargetX() //Метод, позволяющий отдельно получить вычисленную координату цели - x
        {
            return targetCoords.x;
        }

        public double
        getTargetY() //Метод, позволяющий отдельно получить вычисленную координату цели - y
        {
            return targetCoords.y;
        }

        public double
        getTargetZ() //Метод, позволяющий отдельно получить вычисленную координату цели - z
        {
            return targetCoords.z;
        }
    }

    void Update()
    {
        //Создаем объекты классов РЛС, чтобы после их использовать в навигационной системе
        RLS_Active RLS1_Active =
            new RLS_Active(RLS1,
                Drone,
                -90,
                0,
                accuracyDistanceActive,
                accuracyAngleActive);
        RLS_Active RLS2_Active =
            new RLS_Active(RLS2,
                Drone,
                -180,
                -90,
                accuracyDistanceActive,
                accuracyAngleActive);
        RLS_Active RLS3_Active =
            new RLS_Active(RLS3,
                Drone,
                90,
                180,
                accuracyDistanceActive,
                accuracyAngleActive);
        RLS_Active RLS4_Active =
            new RLS_Active(RLS4,
                Drone,
                0,
                90,
                accuracyDistanceActive,
                accuracyAngleActive);
        RLS_Passive RLS5_Passive =
            new RLS_Passive(RLS5,
                Drone,
                accuracyDistancePassive,
                accuracyAnglePassive);
        NavigationSystem Nav =
            new NavigationSystem(RLS1_Active,
                RLS2_Active,
                RLS3_Active,
                RLS4_Active,
                RLS5_Passive);

        Target_X = Nav.getTargetX();
        Target_Y = Nav.getTargetY();
        Target_Z = Nav.getTargetZ();

        Nav_Error =
            Math
                .Sqrt(Math.Pow(Target_X - Drone.position.x, 2) +
                Math.Pow(Target_Y - Drone.position.y, 2) +
                Math.Pow(Target_Z - Drone.position.z, 2)); // Расчет среднеквадратичной ошибки 
    }
}
