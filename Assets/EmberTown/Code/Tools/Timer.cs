using System;


public class Timer
{
    public string TimerLabel { get; private set; }
    public float TimeToWait { get; private set; }

    public float RemainingTime { get; private set; }

    public Action OnTimerEndEvent { get; private set; }

    private bool isRunning = false;





    public void ResetTime()
    {
        RemainingTime = TimeToWait;
        isRunning = false;
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void Tick(float deltaTime)
    {
        if (!isRunning) { return; }

        RemainingTime -= deltaTime;

        if (RemainingTime <= 0f)
        {
            isRunning = false;
            RemainingTime = 0f;

            OnTimerEndEvent?.Invoke();

        }
    }



    // Timer Initializer
    public Timer(string inTimerLabel, float inTimeToWait, Action inOnTimerEndEvent = null)
    {

        TimerLabel = inTimerLabel;

        TimeToWait = inTimeToWait;

        RemainingTime = inTimeToWait;

        OnTimerEndEvent = inOnTimerEndEvent;

    }


}