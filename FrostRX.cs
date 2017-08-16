using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IRXModel{
	void Execute();
	IRXModel ExecuteAfterTime (FrostRX.RXFunc f, float t);
	IRXModel ExecuteWhen (FrostRX.RXFunc f, FrostRX.CondFunc con);
	IRXModel ExecuteWhenStay (FrostRX.RXFunc f, FrostRX.CondFunc con, float t);
	IRXModel Execute (FrostRX.RXFunc f);
	IRXModel ExecuteContinuous (FrostRX.RXFunc f,float t);
}
public class RXModelBase{
	public IRXModel model = null;
	public FrostRX.RXFunc func;
	public IRXModel ExecuteAfterTime(FrostRX.RXFunc f,float t)
	{
		model=new RXTimeModel(f,t);
		return model;
	}
	public IRXModel ExecuteWhen(FrostRX.RXFunc f,FrostRX.CondFunc con)
	{
		model=new RXCondModel(f,con);
		return model;
	}
	public IRXModel ExecuteWhenStay(FrostRX.RXFunc f,FrostRX.CondFunc con,float t)
	{
		model=new RXStayModel(f,con,t);
		return model;
	}
	public IRXModel Execute(FrostRX.RXFunc f)
	{
		model=new RXExecuteModel(f);
		return model;
	}
	public IRXModel ExecuteContinuous (FrostRX.RXFunc f,float t)
	{
		model=new RXContinuousModel(f,t);
		return model;
	}
}
public class RXTimeModel:RXModelBase,IRXModel{
	public float time;
	public RXTimeModel(FrostRX.RXFunc f,float t)
	{
		func = f;
		time = t;
	}
	public void Execute()
	{
		time -= Time.deltaTime;
		if (time <= 0f) {
			func ();
			if (model != null)
				FrostRX.Instance.funcList.Add (model);
			FrostRX.Instance.funcList.Remove (this);
		}
	}
}
public class RXCondModel:RXModelBase,IRXModel{
	public FrostRX.CondFunc cond;
	public RXCondModel(FrostRX.RXFunc f,FrostRX.CondFunc c)
	{
		func = f;
		cond = c;
	}
	public void Execute()
	{
		if (cond()==true) {
			func ();
			if (model != null)
				FrostRX.Instance.funcList.Add (model);
			FrostRX.Instance.funcList.Remove (this);
		}
	}
}
public class RXStayModel:RXModelBase,IRXModel{
	public float time;
	public FrostRX.CondFunc cond;
	private bool _isStay = false;
	private float _time;
	public RXStayModel(FrostRX.RXFunc f,FrostRX.CondFunc c,float t)
	{
		this.func = f;
		this.cond = c;
		time = t;
		this._time = time;
	}
	public void Execute()
	{
		if (cond ()) {
			_isStay = true;
			time -= Time.deltaTime;
			if (time <= 0f) {
				func ();
				if (model != null)
					FrostRX.Instance.funcList.Add (model);
				FrostRX.Instance.funcList.Remove (this);
			}
		} else if(_isStay){
			time = _time;
			_isStay = false;
		}
	}
}
public class RXExecuteModel:RXModelBase,IRXModel{
	public RXExecuteModel(FrostRX.RXFunc f)
	{
		func = f;
	}
	public void Execute()
	{
		func ();
		if (model != null)
			FrostRX.Instance.funcList.Add (model);
		FrostRX.Instance.funcList.Remove (this);
	}
}
public class RXContinuousModel:RXModelBase,IRXModel{
	public float time;
	public RXContinuousModel(FrostRX.RXFunc f,float t)
	{
		func = f;
		time = t;
	}
	public void Execute()
	{
		time -= Time.deltaTime;
		func ();
		if (time <= 0f) {
			if (model != null)
				FrostRX.Instance.funcList.Add (model);
			FrostRX.Instance.funcList.Remove (this);
		}
	}
}

public class FrostRX : UnitySingleton<FrostRX> {
	public delegate void RXFunc();
	public delegate bool CondFunc();
	public List<IRXModel> funcList = new List<IRXModel> ();
	void Start()
	{
		//Test

	}
	public void Update()
	{
		for (int i = funcList.Count-1; i >=0; i--) {
			try {
				funcList [i].Execute ();
			} catch (System.Exception ex) {
				funcList.RemoveAt (i);
			}
		}
	}
	public void Clear()
	{
		funcList.Clear ();
	}
	public IRXModel ExecuteAfterTime(FrostRX.RXFunc f,float t)
	{
		IRXModel model;
		model=new RXTimeModel(f,t);
		funcList.Add (model);
		return model;
	}
	public IRXModel ExecuteWhen(FrostRX.RXFunc f,FrostRX.CondFunc con)
	{
		IRXModel model;
		model=new RXCondModel(f,con);
		funcList.Add (model);
		return model;
	}
	public IRXModel ExecuteWhenStay(FrostRX.RXFunc f,FrostRX.CondFunc con,float t)
	{
		IRXModel model;
		model=new RXStayModel(f,con,t);
		funcList.Add (model);
		return model;
	}
	public IRXModel Execute(FrostRX.RXFunc f)
	{
		IRXModel model;
		model=new RXExecuteModel(f);
		funcList.Add (model);
		return model;
	}
	public IRXModel ExecuteContinuous(FrostRX.RXFunc f,float t)
	{
		IRXModel model;
		model=new RXContinuousModel(f,t);
		funcList.Add (model);
		return model;
	}
}
