using CommonCore.Audio;
using CommonCore.Input;
using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore.SideScrollerGame
{
    /// <summary>
    /// Scene controller for a side scroller scene
    /// </summary>
    public class SideScrollerSceneController : BaseSceneController
    {
        [Header("Side Scroller")]
        public bool UseCameraMovementBounds = false;
        [SerializeField]
        private PlayerController[] Players = null;
        [SerializeField]
        private CameraFollowScript CameraFollowScript = null;

        [Header("Options"), SerializeField]
        private float RespawnTimeout = 10;
        [SerializeField]
        private float SpawnInvulnerability = 5;
        [SerializeField]
        private int SpawnBombs = 10;
        [SerializeField]
        private float StartWaitTime = 5;
        [SerializeField]
        private string SplashText = null;
        [SerializeField]
        private string SplashSound = null;
        [SerializeField]
        private float EndWaitTime = 5;
        [SerializeField]
        private string EndSplashSound = null;

        private Dictionary<PlayerController, DyingPlayer> DyingPlayers = new Dictionary<PlayerController, DyingPlayer>();

        private bool GameEnding = false;
        private Coroutine EndGameCoroutine = null;

        public override void Start()
        {
            base.Start();

            StartCoroutine(CoStartGame());
        }

        public override void Update()
        {
            base.Update();

            HandleMovementBounds();
            HandlePlayerRespawn();
        }

        public void EndLevel()
        {
            StartCoroutine(CoEndLevel());
        }

        private void HandleMovementBounds()
        {
            if(Players != null && Players.Length > 0 && CameraFollowScript != null)
            {
                if(UseCameraMovementBounds)
                {
                    if (CameraFollowScript.FollowPlayer)
                        Debug.LogWarning("Camera is set to follow player, but player is limited to camera bounds, which will make Weird Shit happen!");

                    var bounds = CameraFollowScript.GetCameraBounds();
                    foreach (var player in Players)
                        player.MovementBounds = bounds;
                }
                else
                {
                    foreach (var player in Players)
                        player.MovementBounds = Rect.zero;
                }
            }
        }

        private void HandlePlayerRespawn()
        {
            if (GameEnding) //can't respawn if game is over
                return;

            //countdown dying players
            int deadPlayers = 0;
            foreach (var dpKvp in DyingPlayers)
            {
                if (dpKvp.Value.TimeLeft > 0)
                {
                    dpKvp.Value.TimeLeft -= Time.deltaTime;
                }
                else
                {
                    deadPlayers++;
                }
            }

            //handle ALL players dead
            if(deadPlayers == Players.Length)
            {
                GameEnding = true;
                EndGameCoroutine = StartCoroutine(CoEndGame());
            }

            //scan for dead players
            foreach (var player in Players)
            {
                if(player.PlayerIsDead && !DyingPlayers.ContainsKey(player))
                {
                    var countdownGO = Instantiate(CoreUtils.LoadResource<GameObject>("UI/RespawnCountdown"), CoreUtils.GetUIRoot());
                    var countdownController = countdownGO.GetComponent<CountdownController>();
                    countdownController.SetupCountdown(RespawnTimeout);
                    DyingPlayers.Add(player, new DyingPlayer() { TimeLeft = RespawnTimeout, CountdownController = countdownController });
                }
            }

            //resurrect dead players if requested
            //note that we'll never actually destroy the player objects so we can simply resurrect them
            //note that this is set up for two players instead of n players
            
            if(MappedInput.GetButtonDown("Start") && MetaState.Instance.Player1Credits >= 1 && Players.Length >= 1 && Players[0] != null && DyingPlayers.ContainsKey(Players[0]) && DyingPlayers[Players[0]].TimeLeft > 0)
            {
                var dp = DyingPlayers[Players[0]];
                DyingPlayers.Remove(Players[0]);
                Destroy(dp.CountdownController.gameObject);
                Players[0].Resurrect();
                Players[0].SetSpawnInvulnerability(SpawnInvulnerability);
                Players[0].Bombs = SpawnBombs;
                MetaState.Instance.Player1Credits--;
            }

            /*
            if (MappedInput.GetButtonDown("StartP2") && MetaState.Instance.Player2Credits >= 1 && Players.Length >= 2 && Players[1] != null && DyingPlayers.ContainsKey(Players[1]) && DyingPlayers[Players[1]].TimeLeft > 0)
            {
                var dp = DyingPlayers[Players[1]];
                DyingPlayers.Remove(Players[1]);
                Destroy(dp.CountdownController.gameObject);
                Players[1].Resurrect();
                Players[1].SetSpawnInvulnerability(SpawnInvulnerability);
                Players[1].Bombs = SpawnBombs;
                MetaState.Instance.Player2Credits--;
            }
            */
        }

        //start game sequence
        private IEnumerator CoStartGame()
        {
            //set invulnerability, disable player control
            foreach (var player in Players)
            {
                player.Bombs = SpawnBombs;
                player.SetSpawnInvulnerability(SpawnInvulnerability + StartWaitTime);
                player.PlayerInControl = false;
                player.SetSpawnAnimation();
            }

            var splashGO = Instantiate(CoreUtils.LoadResource<GameObject>("UI/MissionStartSplash"), CoreUtils.GetUIRoot());
            var splash = splashGO.GetComponent<MissionStartSplashController>();
            splash.SetupSplash(StartWaitTime, SplashText, SplashSound);

            yield return new WaitForSeconds(StartWaitTime);

            //return control to player
            foreach (var player in Players)
            {
                player.PlayerInControl = true;
            }

        }

        //end game sequence
        private IEnumerator CoEndGame()
        {
            AudioPlayer.Instance.PlayUISound("cine_gameover");
            ScreenFader.FadeTo(Color.black, 5f, false, false, false);
            for(float elapsed = 0; elapsed < 1f; elapsed += Time.deltaTime)
            {
                AudioPlayer.Instance.SetMusicVolume(1f - (elapsed / 1f), MusicSlot.Ambient); //fade out music
                yield return null;
            }
            yield return new WaitForSeconds(4f);

            //yield return null;

            MetaState.Instance.NextScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene("GameOverScene");
            //SharedUtils.EndGame();
        }

        private IEnumerator CoEndLevel()
        {
            foreach (var player in Players)
            {
                player.PlayerInControl = false;
                player.PlayerIsInvulnerable = true;
            }

            AudioPlayer.Instance.PlayUISound("cine_missionwin");
            var splashGO = Instantiate(CoreUtils.LoadResource<GameObject>("UI/MissionEndSplash"), CoreUtils.GetUIRoot());
            var splash = splashGO.GetComponent<MissionStartSplashController>();
            splash.SetupSplash(EndWaitTime, SplashText, EndSplashSound);
            ScreenFader.FadeTo(Color.black, 5f, false, false, false);

            for (float elapsed = 0; elapsed < 1f; elapsed += Time.deltaTime)
            {
                AudioPlayer.Instance.SetMusicVolume(1f - (elapsed / 1f), MusicSlot.Ambient); //fade out music
                yield return null;
            }
            yield return new WaitForSeconds(Mathf.Max(EndWaitTime-1f, 1f));

            //end level
            SharedUtils.EndGame();
        }

        private class DyingPlayer
        {
            //we may eventually put more things here
            public float TimeLeft;
            public CountdownController CountdownController;
        }
    }
}