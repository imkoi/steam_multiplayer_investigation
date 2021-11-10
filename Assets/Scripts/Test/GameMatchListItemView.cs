using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VoxCake.Extensions;
using VoxCake.Network;

namespace Test
{
	public class GameMatchListItemView : MonoBehaviour
	{
		[SerializeField] private Button _joinButton;
		[SerializeField] private Button _dataButton;
		[SerializeField] private Text _playerNameText;
		[SerializeField] private Text _playerCountText;

		private CancellationTokenSource _cancellationTokenSource;
		private NetworkMatchInfo _matchInfo;
		private NetworkClient _networkClient;

		private void OnEnable()
		{
			_cancellationTokenSource = new CancellationTokenSource();
		}

		private void OnDisable()
		{
			_joinButton.onClick.RemoveListener(OnJoinButtonClicked);
			_dataButton.onClick.RemoveListener(OnDataButtonClicked);
			_cancellationTokenSource.Cancel();
		}

		public void Initialize(NetworkMatchInfo matchInfo, NetworkClient networkClient)
		{
			_matchInfo = matchInfo;
			_networkClient = networkClient;
			
			_playerCountText.text = $"{matchInfo.PlayersCount}/{matchInfo.MaxPlayersCount}";
			//_playerNameText.text = $"{matchInfo.Owner.Name}";
			
			_joinButton.onClick.AddListener(OnJoinButtonClicked);
			_dataButton.onClick.AddListener(OnDataButtonClicked);
		}

		private async void OnJoinButtonClicked()
		{
			var matchProtocol = new GameProtocol();
			var match = await _networkClient.JoinMatchAsync(_matchInfo, matchProtocol, _cancellationTokenSource.Token);
		}

		private void OnDataButtonClicked()
		{
			foreach (var pair in _matchInfo.Data)
			{
				Debug.Log($"{pair.Key}:{pair.Value}");
			}
		}
	}
}