using System;
using UnityEngine;
using UnityEngine.UI;
using VoxCake.Network;

namespace Test
{
	public class GameMatchListItemView : MonoBehaviour, IDisposable
	{
		public event Action<NetworkMatchInfo> JoinButtonClicked;
		public event Action<NetworkMatchInfo> DataButtonClicked;
		
		[SerializeField] private Button _joinButton;
		[SerializeField] private Button _dataButton;
		[SerializeField] private Text _playerNameText;
		[SerializeField] private Text _playerCountText;

		private NetworkMatchInfo _matchInfo;

		public void Initialize(NetworkMatchInfo matchInfo)
		{
			_matchInfo = matchInfo;

			_playerCountText.text = $"{matchInfo.PlayersCount}/{matchInfo.MaxPlayersCount}";

			_joinButton.onClick.AddListener(OnJoinButtonClicked);
			_dataButton.onClick.AddListener(OnDataButtonClicked);
		}

		public void Dispose()
		{
			_joinButton.onClick.RemoveListener(OnJoinButtonClicked);
			_dataButton.onClick.RemoveListener(OnDataButtonClicked);
			
			Destroy(gameObject);
		}

		private void OnJoinButtonClicked()
		{
			JoinButtonClicked?.Invoke(_matchInfo);
		}
		
		private void OnDataButtonClicked()
		{
			DataButtonClicked?.Invoke(_matchInfo);
		}
	}
}