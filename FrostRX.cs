using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IRXModel{
	void Execute();
	IRXModel ExecuteAfterTime (FrostRX.RXFunc f, float t);
	IRXModel ExecuteWhen (FrostRX.RXFunc f, FrostRX.CondFunc con);
}
public class RXModelBase{
	public IRXModel model = null;
	public FrostRX.RXFunc func;
	public IRXModel ExecuteAfterTime(FrostRX.RXFunc f,float t)
	{
		model=new RXTimeModel(){func = f,time = t};
		return model;
	}
	public IRXModel ExecuteWhen(FrostRX.RXFunc f,FrostRX.CondFunc con)
	{
		model=new RXCondModel(){func = f,cond = con};
		return model;
	}
}
public class RXTimeModel:RXModelBase,IRXModel{
	public float time;
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
public partial class FrostRX : UnitySingleton<FrostRX> {
	public delegate void RXFunc();
	public delegate bool CondFunc();
	public List<IRXModel> funcList = new List<IRXModel> ();
	void Start()
	{
		//Test
		ExecuteAfterTime (() => {
			Debug.Log(1f);
		},1f).ExecuteAfterTime (() => {
			Debug.Log(2f);
		},1f).ExecuteAfterTime (() => {
			Debug.Log(3f);
		},1f);
	}
	public RXTimeModel ExecuteAfterTime(RXFunc f,float t)
	{
		RXTimeModel model = new RXTimeModel (){ func = f, time = t };
		funcList.Add (model);
		return model;
	}
	public void Update()
	{
		for (int i = funcList.Count-1; i >=0; i--) {
			funcList [i].Execute ();
		}
	}
}
