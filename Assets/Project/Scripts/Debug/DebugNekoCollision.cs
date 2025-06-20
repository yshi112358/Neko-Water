using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using Game.Neko;

namespace Game.NekoDebug
{
	public class DebugNekoCollision : MonoBehaviour
	{
		private DebugSwitch _debugSwitch => FindObjectOfType<DebugSwitch>();
		private ReactiveCollection<NekoData> _nekoCollisionList => GetComponent<NekoCollisionList>().nekoCollisionList;
		private List<GameObject> lineList = new List<GameObject>();

		void Awake()
		{
			Debug.Log("DebugNekoCollision");
			this.UpdateAsObservable()
				.TakeUntilDisable(this.gameObject)
				.Subscribe(_ =>
				{
					foreach (var nekoData in _nekoCollisionList)
					{
						Vector3[] positions = new Vector3[]{
							(nekoData.gameObject.transform.GetChild(0).position+nekoData.gameObject.transform.GetChild(4).position)/2f,          // 開始点
							(transform.GetChild(0).position+transform.GetChild(4).position)/2f               // 終了点
						};
						lineList.Where(line => line.name == nekoData.gameObject.GetInstanceID().ToString()).First().GetComponent<LineRenderer>().SetPositions(positions);
					}
				}, () =>
				{
					lineList.ForEach(line => Destroy(line));
				});
			_nekoCollisionList.ObserveAdd()
				.Subscribe(nekoData =>
				{
					var line = new GameObject();
					line.name = nekoData.Value.gameObject.GetInstanceID().ToString();
					lineList.Add(line);
					var linerend = line.gameObject.AddComponent<LineRenderer>();
					linerend.startWidth = 0.1f;                   // 開始点の太さを0.1にする
					linerend.endWidth = 0.1f;
					linerend.sortingOrder = 10;

					if (_debugSwitch.isDebug.Value)
						linerend.enabled = true;
					else
						linerend.enabled = false;
				})
				.AddTo(this);
			_nekoCollisionList.ObserveRemove()
				.Subscribe(nekoData =>
				{
					var line = lineList.Where(line => line.name == nekoData.Value.gameObject.GetInstanceID().ToString()).First();
					lineList.Remove(line);
					Destroy(line);
				})
				.AddTo(this);

			_debugSwitch.isDebug
				.Subscribe(x =>
				{
					foreach (var line in lineList)
					{
						line.GetComponent<LineRenderer>().enabled = x;
					}
				})
				.AddTo(this);
		}
	}
}