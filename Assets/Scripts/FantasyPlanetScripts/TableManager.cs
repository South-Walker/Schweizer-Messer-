﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityScript;
using Assets.Scripts.FantasyPlanetScripts;

public class TableManager : MonoBehaviour {
    private DateTime date;
    private List<Classob> classToday;
    private Animator a_DetailedTableCurrent;
    private Transform t_DetailedWindow;
    private Transform t_Window;
    private Transform t_Title;
    private Transform t_Table;
    private Button b_Close;
    private Animator a_Open;
    private string[] Weekdays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
    List<Transform> timePeriod = new List<Transform>();
    List<Transform> Times = new List<Transform>();
    const string OpenTransitionName = "Open";
    const string ClosedStateName = "Closed";
    // Use this for initialization
    void Start () {
    }
    private void OnEnable()
    {
    }
    #region Event
    public void CloseCurrent()
    {
        if (a_Open == null || !a_Open.isActiveAndEnabled)
            return;

        a_Open.SetBool(OpenTransitionName, false);
        StartCoroutine(DisablePanelDeleyed(a_Open));
        a_Open = null;

    }
    public void CloseCurrentDetailedWindow()
    {
        a_DetailedTableCurrent.SetBool(OpenTransitionName, false);
    }
    public void FillDetailedWindow(Classob nowClass)
    {
        t_DetailedWindow.Find("ClassName").GetComponent<Text>().text = nowClass.classname;
        t_DetailedWindow.Find("Teacher").GetComponent<Text>().text = "教师：" + nowClass.teacher;
        t_DetailedWindow.Find("Classroom").GetComponent<Text>().text = "教室：" + nowClass.room;
        t_DetailedWindow.Find("Time").GetComponent<Text>().text = "时间：" + nowClass.alldate;
        t_DetailedWindow.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
        {
            CloseCurrentDetailedWindow();
        });
        a_DetailedTableCurrent.SetBool(OpenTransitionName, true);
    }
    #endregion
    public void Initialize(DateTime date, ClassTableob classtable)
    {
        this.date = date;
        this.classToday = classtable.GetClassToday(date);
        
        catchElement();

        this.b_Close = t_Window.Find("Close").GetComponent<Button>();
        b_Close.onClick.AddListener(() =>
        {
            CloseCurrent();
        });
        fillClassTable();
    }
    private void catchElement()
    {
        t_Window = transform.Find("Window");
        this.t_DetailedWindow = t_Window.Find("DetailedWindow");
        this.t_Title = t_Window.Find("Title");
        this.t_Table = t_Window.Find("Table");
        this.a_DetailedTableCurrent = t_Window.GetComponent<Animator>();
        a_Open = GetComponent<Animator>();

        timePeriod.Add(t_Table.Find("Morning"));
        timePeriod.Add(t_Table.Find("Afternoon"));
        timePeriod.Add(t_Table.Find("Evening"));
        foreach (var item in timePeriod)
        {
            Times.Add(item.Find("FirstClass"));
            Times.Add(item.Find("SecondClass"));
        }
    }
    private void fillClassTable()
    {
        t_Title.Find("Date").GetComponent<Text>().text = date.ToString("yyyy/MM/dd");
        t_Title.Find("WeekDay").GetComponent<Text>().text = Weekdays[(int)date.DayOfWeek];
        foreach (var classob in classToday)
        {
            int beginPoint = classob.timebegin / 2;
            int endPoint = (classob.timeend - classob.timebegin + 1) / 2;
            var position = Times.GetRange(beginPoint, endPoint);
            foreach (var item in position)
            {
                var button = item.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    FillDetailedWindow(classob);
                });
                item.Find("Text").GetComponent<Text>().text = classob.classname;
            }
        }
    }
    IEnumerator DisablePanelDeleyed(Animator anim)
    {
        bool closedStateReached = false;
        bool wantToClose = true;
        while (!closedStateReached && wantToClose)
        {
            if (!anim.IsInTransition(0))
                closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName(ClosedStateName);

            wantToClose = !anim.GetBool(OpenTransitionName);

            yield return new WaitForEndOfFrame();
        }

        if (wantToClose)
            anim.gameObject.SetActive(false);
    }
}
