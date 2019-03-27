using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IRXModel
{
    void Execute();
    void SetRxId(int rxId);
	IRXModel Wait(float t);
    IRXModel ExecuteAfterTime(FrostRX.RXFunc f, float t);
    IRXModel ExecuteUntil(FrostRX.RXFunc f, FrostRX.CondFunc con);
    IRXModel ExecuteWhen(FrostRX.RXFunc f, FrostRX.CondFunc con);
    IRXModel ExecuteWhenStay(FrostRX.RXFunc f, FrostRX.CondFunc con, float t);
    IRXModel Execute(FrostRX.RXFunc f);
    IRXModel ExecuteContinuous(FrostRX.RXFunc f, float t);
    IRXModel GoToBegin();
    int GetId();
}
public class RXModelBase:IRXModel
{
    public int rxId;
    public IRXModel model = null;
    public FrostRX.RXFunc func;
    public virtual void Execute()
    {

    }
    public void SetRxId(int rxId)
    {
        this.rxId = rxId;
    }
    public int GetId()
    {
        return rxId;
    }
    protected void TryNext()
    {
        if (model != null)
        {
            FrostRX.Instance.funcList.Add(model);
        }
        else
        {
            FrostRX.Instance.EndRxById(rxId);
        }
        FrostRX.Instance.funcList.Remove(this);
    }
    public IRXModel GoToBegin()
    {
        model = new RXGoToBegin();
        model.SetRxId(rxId);
        return model;
    }
	public IRXModel Wait(float time)
	{
		model = new RXWaitModel(time);
		model.SetRxId(rxId);
		return model;
	}
    public IRXModel ExecuteAfterTime(FrostRX.RXFunc f, float t)
    {
        model = new RXTimeModel(f, t);
        model.SetRxId(rxId);
        return model;
    }
    public IRXModel ExecuteWhen(FrostRX.RXFunc f, FrostRX.CondFunc con)
    {
        model = new RXCondModel(f, con);
        model.SetRxId(rxId);
        return model;
    }
    public IRXModel ExecuteUntil(FrostRX.RXFunc f, FrostRX.CondFunc con)
    {
        model = new RXUntilModel(f, con);
        model.SetRxId(rxId);
        return model;
    }
    public IRXModel ExecuteWhenStay(FrostRX.RXFunc f, FrostRX.CondFunc con, float t)
    {
        model = new RXStayModel(f, con, t);
        model.SetRxId(rxId);
        return model;
    }
    public IRXModel Execute(FrostRX.RXFunc f)
    {
        model = new RXExecuteModel(f);
        model.SetRxId(rxId);
        return model;
    }
    public IRXModel ExecuteContinuous(FrostRX.RXFunc f, float t)
    {
        model = new RXContinuousModel(f, t);
        model.SetRxId(rxId);
        return model;
    }
}
public class RXRoot : RXModelBase
{
    public RXRoot(int rxId)
    {
        this.rxId = rxId;
    }
    public sealed override void Execute()
    {
        TryNext();
    }
}
public class RXGoToBegin : RXModelBase
{
    public sealed override void Execute()
    {
        FrostRX.Instance.RestartRxById(rxId);
        if (model != null)
        {
            FrostRX.Instance.funcList.Add(model);
        }
    }
}
public class RXTimeModel : RXModelBase
{
    float waitTime;
    float time;
    public RXTimeModel(FrostRX.RXFunc f, float t)
    {
        waitTime = t;
        func = f;
        time = t;
    }
    public sealed override void Execute()
    {
        time -= Time.deltaTime;
        if (time <= 0f)
        {
            time = waitTime;
            func();
            TryNext();
        }
    }
}
public class RXCondModel : RXModelBase
{
    public FrostRX.CondFunc cond;
    public RXCondModel(FrostRX.RXFunc f, FrostRX.CondFunc c)
    {
        func = f;
        cond = c;
    }
    public sealed override void Execute()
    {
        if (cond() == true)
        {
            func();
            TryNext();
        }
    }
}
public class RXUntilModel : RXModelBase
{
    public FrostRX.CondFunc cond;
    public RXUntilModel(FrostRX.RXFunc f, FrostRX.CondFunc c)
    {
        func = f;
        cond = c;
    }
    public sealed override void Execute()
    {
        if (cond() == true)
        {
            TryNext();
        }
        else
        {
            func();
        }
    }
}
public class RXStayModel : RXModelBase
{
    public float time;
    public FrostRX.CondFunc cond;
    private bool _isStay = false;
    private float _time;
    public RXStayModel(FrostRX.RXFunc f, FrostRX.CondFunc c, float t)
    {
        this.func = f;
        this.cond = c;
        time = t;
        this._time = time;
    }
    public sealed override void Execute()
    {
        if (cond())
        {
            _isStay = true;
            time -= Time.deltaTime;
            if (time <= 0f)
            {
                func();
                TryNext();
				time = _time;
				_isStay = false;
            }
        }
        else if (_isStay)
        {
            time = _time;
            _isStay = false;
        }
    }
}
public class RXExecuteModel : RXModelBase
{
    public RXExecuteModel(FrostRX.RXFunc f)
    {
        func = f;
    }
    public sealed override void Execute()
    {
        func();
        TryNext();
    }
}
public class RXContinuousModel : RXModelBase
{
	float waitTime;
    public float time;
    public RXContinuousModel(FrostRX.RXFunc f, float t)
    {
        func = f;
        waitTime = time = t;
    }
    public sealed override void Execute()
    {
        time -= Time.deltaTime;
        func();
        if (time <= 0f)
        {
            TryNext();
			time = waitTime;
        }
    }
}
public class RXWaitModel : RXModelBase
{
	float waitTime;
	public float time;
	public RXWaitModel(float t)
	{
		waitTime = time = t;
	}
	public sealed override void Execute()
	{
		time -= Time.deltaTime;
		if (time <= 0f)
		{
			TryNext();
			time = waitTime;
		}
	}
}
public class FrostRX : Singleton<FrostRX>
{
    public delegate void RXFunc();
    public delegate bool CondFunc();
    private Dictionary<int, IRXModel> _rxDic = new Dictionary<int, IRXModel>();
    public List<IRXModel> funcList = new List<IRXModel>();
    int rxId = 0;
    public IRXModel StartRX()
    {
        IRXModel model;
        int id = GetRxId();
        model = new RXRoot(id);
        _rxDic.Add(id,model);
        funcList.Add(model);
        return model;
    }
    public void EndRxById(int rxId)
    {
        if (_rxDic.ContainsKey(rxId))
            _rxDic.Remove(rxId);
        for (int i = 0; i < funcList.Count; i++)
        {
            if (funcList[i].GetId() == rxId)
            {
                funcList.RemoveAt(i);
                i--;
            }
        }
    }
    public void RestartRxById(int rxId)
    {
		for (int i = 0; i < funcList.Count; i++)
		{
			if (funcList[i].GetId() == rxId)
			{
				funcList.RemoveAt(i);
				i--;
			}
		}
        if (_rxDic.ContainsKey(rxId))
            funcList.Add(_rxDic[rxId]);
    }
    int GetRxId()
    {
        while (_rxDic.ContainsKey(rxId))
        {
            rxId++;
        }
        return rxId++;
    }
    public void Execute()
    {
        for (int i = funcList.Count - 1; i >= 0; i--)
        {
            try
            {
                funcList[i].Execute();
            }
            catch (System.Exception ex)
            {
                funcList.RemoveAt(i);
            }
        }
    }
    public void Clear()
    {
        funcList.Clear();
        _rxDic.Clear();
    }
}
