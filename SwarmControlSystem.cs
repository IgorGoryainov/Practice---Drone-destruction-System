using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmControlSystem : MonoBehaviour
{
    public Transform emtyObjectTargetPos; //Пустой объект, необходим для хранения целевой точки

    public float swarmSpeed; // Вычисленная скорость роя

    public Transform Drone_1; // Объекты дронов роя

    public Transform Drone_2;

    public Transform Drone_3;

    public Transform Drone_4;

    public Transform Drone_5;

    public Transform Drone_6;

    public Transform Drone_7;

    public Transform Drone_8;

    public Transform Drone_9;

    public Transform Drone_10;

    public Transform Drone_11;

    public Transform Drone_12;

    public Transform Drone_13;

    public Transform Drone_14;

    public Transform Drone_15;

    public Vector3 pos;

    public struct Coords
    {
        public double x;

        public double y;

        public double z;
    } // Структура для удобного хранения координат целевой и текущей точки положения

    void Start()
    {
        //В данном методе, мы просто расставляем коптеры на земле в форме окружности
        Coords[] DronesCoords = new Coords[15]; //Создаем 15 точек в трехмерном пр-ве

        Transform[] DronesList =
        {
            Drone_1,
            Drone_2,
            Drone_3,
            Drone_4,
            Drone_5,
            Drone_6,
            Drone_7,
            Drone_8,
            Drone_9,
            Drone_10,
            Drone_11,
            Drone_12,
            Drone_13,
            Drone_14,
            Drone_15
        }; //Создаем массив из объектов дронов, для удобного обращения к каждому дрону (т.е. просто через индексы)

        float x_O1_rel_O = 56.0f; // Координаты нулевой точки СК связанной с роем относительно общей (основной) СК
        float y_O1_rel_O = 0.9f;
        float z_O1_rel_O = 108.0f; 

        double x_Drone_rel_O1 = 0f; // Координаты дрона относительно нулевой точки СК связанной с роем
        double y_Drone_rel_O1 = 0f; 
        double z_Drone_rel_O1 = 0f; 

        double swarmRadius = 5f; // Радиус окружности, по которой располагаем дроны перед взлетом
        double circleAngle = 0f; // Счетчик градусов для окружности
        double angleStep = 2 * Math.PI / 15; //Шаг, для равномерного расположения дронов по окружности

        for (int i = 0; i < 15; ++i) //Перебираем каждый коптер и рассчитываем для каждого свои координаты
        {
            x_Drone_rel_O1 = swarmRadius * Math.Cos(circleAngle); //Расчет координат коптера, относительно связанной с роем СК
            z_Drone_rel_O1 = swarmRadius * Math.Sin(circleAngle);

            DronesCoords[i].x = x_Drone_rel_O1;  
            DronesCoords[i].y = y_Drone_rel_O1;
            DronesCoords[i].z = z_Drone_rel_O1;

            DronesCoords[i].x += x_O1_rel_O; //Прибавляем к вычисленным относительным координатам, смещение связанной СК относительно основной СК
            DronesCoords[i].y += y_O1_rel_O;
            DronesCoords[i].z += z_O1_rel_O;

            DronesList[i].position =
                new Vector3((float) DronesCoords[i].x,
                    (float) DronesCoords[i].y,
                    (float) DronesCoords[i].z); // Располагаем дроны по вычисленным координатам
            circleAngle += angleStep; //делаем шаг, для следующего дрона
        }
        pos = DronesList[0].position; //считываем первонаальное положение 1-го дрона
    }

    void FixedUpdate()
    {
        // FixedUpdate - не требует Time.deltaTime
        Coords[] DronesCoords = new Coords[15]; 

        Transform[] DronesList =
        {
            Drone_1,
            Drone_2,
            Drone_3,
            Drone_4,
            Drone_5,
            Drone_6,
            Drone_7,
            Drone_8,
            Drone_9,
            Drone_10,
            Drone_11,
            Drone_12,
            Drone_13,
            Drone_14,
            Drone_15
        };

        double target_x = FindObjectOfType<Distance>().Target_X; // Получаем координаты цели из навигационной системы
        double target_y = FindObjectOfType<Distance>().Target_Y;
        double target_z = FindObjectOfType<Distance>().Target_Z;

        swarmSpeed = (DronesList[0].position - pos).magnitude / Time.deltaTime; //Расчет скорости роя
        pos = DronesList[0].position; // Сохранения текущего положения, чтобы на следующей итерации рассчитать скорость

        if ((target_x != 0) & (pos.y < 9)) //Проверка, если нав.система обнаружила дрон, и при этом рой еще не взлетел
        {
            //Взлетная конфигурация
            float x_O1_rel_O = 56.0f; // Координаты нулевой точки СК связанной с роем
            float y_O1_rel_O = 10.0f;
            float z_O1_rel_O = 108.0f;

            double x_Drone_rel_O1 = 0f; // Координаты дрона относительно нулевой точки СК связанной с роем
            double y_Drone_rel_O1 = 0f;
            double z_Drone_rel_O1 = 0f;

            double swarmRadius = 5f; //Радиус окружности, в которую располагаем рой
            double circleAngle = 0f; //Счетчик угла, для равномерного расположения дронов
            double angleStep = 2 * Math.PI / 15; //Шаг по окружности
            for (int i = 0; i < 15; ++i) //Реализация взлета
            {
                x_Drone_rel_O1 = swarmRadius * Math.Cos(circleAngle); //То же самое, что и в void start()
                z_Drone_rel_O1 = swarmRadius * Math.Sin(circleAngle); 

                DronesCoords[i].x = x_Drone_rel_O1;
                DronesCoords[i].y = y_Drone_rel_O1;
                DronesCoords[i].z = z_Drone_rel_O1;

                DronesCoords[i].x += x_O1_rel_O;
                DronesCoords[i].y += y_O1_rel_O;
                DronesCoords[i].z += z_O1_rel_O;
                emtyObjectTargetPos.position =
                    new Vector3((float) DronesCoords[i].x,
                        (float) DronesCoords[i].y,
                        (float) DronesCoords[i].z); //задача целевой точки
                DronesList[i].position =
                    Vector3
                        .Lerp(DronesList[i].position,
                        emtyObjectTargetPos.position,
                        0.01f); //Направление дрона на целевую точку
                circleAngle += angleStep;
            }

            //Взлетная конфигурация
        }

        if ((target_x != 0) & (pos.y >= 9)) //Проверка, если нав.система обнаружила цель и при этом рой уже взлетел
        {
            float speed = 0.5f; //скорость сближения роя с целью
            float smooth = 1 - Mathf.Pow(0.5f, Time.deltaTime * speed); //махинации для нормальной работы Lerp

            target_x = FindObjectOfType<Distance>().Target_X; //Снова получаем координаты цели от нав. системы
            target_y = FindObjectOfType<Distance>().Target_Y;
            target_z = FindObjectOfType<Distance>().Target_Z;

            //Полетная конфигурация
            double x_O1_rel_O = target_x; //Теперь у нас координаты начала связанной с роем СК - координаты цели 
            double y_O1_rel_O = target_y;
            double z_O1_rel_O = target_z;

            double x_Drone_rel_O1 = 0f; //Будем менять по ходу распределения координат
            double y_Drone_rel_O1 = 0f;
            double z_Drone_rel_O1 = 0f;

            double swarmRadius_1 = 1.5f; //Радуис второго фронта
            double circleAngle_1 = 0f; 
            double angleStep_1 = 2 * Math.PI / 5; //Распределяем 5 дронов во втором фронту

            double swarmRadius_2 = 2.5f; // Радиус третьего фронта
            double circleAngle_2 = 0f; 
            double angleStep_2 = 2 * Math.PI / 9; //Распределяем 9 дронов по третьему фронту

            for (int i = 0; i < 5; ++i) //распределение дронов по второму фронту
            {
                x_Drone_rel_O1 = swarmRadius_1 * Math.Cos(circleAngle_1);
                y_Drone_rel_O1 = swarmRadius_1 * Math.Sin(circleAngle_1);

                z_Drone_rel_O1 = -2f; //отвечает за расстояние между фронтами

                DronesCoords[i].x = x_Drone_rel_O1;
                DronesCoords[i].y = y_Drone_rel_O1;
                DronesCoords[i].z = z_Drone_rel_O1;

                DronesCoords[i].x += x_O1_rel_O;
                DronesCoords[i].y += y_O1_rel_O;
                DronesCoords[i].z += z_O1_rel_O;
                emtyObjectTargetPos.position =
                    new Vector3((float) DronesCoords[i].x,
                        (float) DronesCoords[i].y,
                        (float) DronesCoords[i].z);
                DronesList[i].position =
                    Vector3
                        .Lerp(DronesList[i].position,
                        emtyObjectTargetPos.position,
                        smooth);
                circleAngle_1 += angleStep_1;
            }
            for (int i = 5; i < 14; ++i) //распределение дронов по третьему фронту
            {
                x_Drone_rel_O1 = swarmRadius_2 * Math.Cos(circleAngle_2);
                y_Drone_rel_O1 = swarmRadius_2 * Math.Sin(circleAngle_2);

                z_Drone_rel_O1 = -4f; //отвечает за расстояние между фронтами

                DronesCoords[i].x = x_Drone_rel_O1;
                DronesCoords[i].y = y_Drone_rel_O1;
                DronesCoords[i].z = z_Drone_rel_O1;

                DronesCoords[i].x += x_O1_rel_O;
                DronesCoords[i].y += y_O1_rel_O;
                DronesCoords[i].z += z_O1_rel_O;
                emtyObjectTargetPos.position =
                    new Vector3((float) DronesCoords[i].x,
                        (float) DronesCoords[i].y,
                        (float) DronesCoords[i].z);
                DronesList[i].position =
                    Vector3
                        .Lerp(DronesList[i].position,
                        emtyObjectTargetPos.position,
                        smooth);
                circleAngle_2 += angleStep_2;
            }

            //Первый фронт
            emtyObjectTargetPos.position =
                new Vector3((float) x_O1_rel_O,
                    (float) y_O1_rel_O,
                    (float) z_O1_rel_O);
            DronesList[14].position =
                Vector3
                    .Lerp(DronesList[14].position,
                    emtyObjectTargetPos.position,
                    smooth);
            //Полетная конфигурация
        }
    }
}
