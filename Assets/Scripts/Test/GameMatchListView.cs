using UnityEngine;
using VoxCake.Network;

namespace Test
{
	public class GameMatchListView : MonoBehaviour
	{
		[SerializeField] private RectTransform _contentTransform;
		[SerializeField] private GameMatchListItemView _itemPrefab;
		
		public void Initialize(NetworkMatchInfo[] matchInfos, NetworkClient networkClient)
		{
			foreach (var matchInfo in matchInfos)
			{
				var item = Instantiate(_itemPrefab, _contentTransform);
				item.Initialize(matchInfo, networkClient);
			}
		}
	}
}