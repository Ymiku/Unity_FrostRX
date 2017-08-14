# FrostRX
Example:

ExecuteAfterTime (() => {
			Debug.Log (1f);
		}, 1f).
		ExecuteAfterTime (() => {
			Debug.Log (2f);
		}, 1f).
		ExecuteAfterTime (() => {
			Debug.Log (3f);
		}, 1f).
		ExecuteWhen (() => {
			Debug.Log (4f);
		}, () => {
			return a==1;
		});
