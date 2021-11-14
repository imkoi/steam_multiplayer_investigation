using System;
using System.Collections.Generic;
using UnityEngine;
using VoxCake.Network;

namespace Test
{
	public class GameMatchListView : MonoBehaviour
	{
		public event Action<NetworkMatchInfo> JoinButtonClicked;
		public event Action<NetworkMatchInfo> DataButtonClicked;
		
		[SerializeField] private RectTransform _contentTransform;
		[SerializeField] private GameMatchListItemView _itemPrefab;

		private List<GameMatchListItemView> _items = new List<GameMatchListItemView>();
		
		public void Refresh(NetworkMatchInfo[] matchInfos)
		{
			foreach (var item in _items)
			{
				item.JoinButtonClicked -= OnJoinButtonClicked;
				item.DataButtonClicked -= OnDataButtonClicked;
				
				item.Dispose();
			}
			
			_items.Clear();

			foreach (var matchInfo in matchInfos)
			{
				var item = Instantiate(_itemPrefab, _contentTransform);
				item.Initialize(matchInfo);

				item.JoinButtonClicked += OnJoinButtonClicked;
				item.DataButtonClicked += OnDataButtonClicked;
				
				_items.Add(item);
			}
		}
		
		private void OnJoinButtonClicked(NetworkMatchInfo matchInfo)
		{
			JoinButtonClicked?.Invoke(matchInfo);
		}
    
		private void OnDataButtonClicked(NetworkMatchInfo matchInfo)
		{
			DataButtonClicked?.Invoke(matchInfo);
		}
	}
}