namespace AxitUnityTemplate.UI.Classic.Async
{
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.EventSystems;
    using Cysharp.Threading.Tasks;

    public class UiTransition : MonoBehaviour
    {
        [SerializeField] private PlayableDirector   intro;
        [SerializeField] private PlayableDirector   outro;
        [SerializeField] private DirectorUpdateMode timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;

        private EventSystem eventSystem;
        private bool        isPlaying;

        private void Awake()
        {
            this.eventSystem = EventSystem.current;

            foreach (var director in new[] { this.intro, this.outro })
            {
                if (!director.playableAsset) break;
                director.time = 0f;
                director.Evaluate();
                director.timeUpdateMode = this.timeUpdateMode;
                director.playOnAwake    = false;
            }
        }

        public async UniTask PlayIntroAnimationAsync()
        {
            this.SetInputLock(false);
            await this.PlayAnimationAsync(this.intro);
        }

        public async UniTask PlayOutroAnimationAsync()
        {
            await this.PlayAnimationAsync(this.outro);
            this.SetInputLock(true);
        }

        private async UniTask PlayAnimationAsync(PlayableDirector animationPlay)
        {
            if (this.isPlaying || !animationPlay) return;
            this.isPlaying = true;

            animationPlay.Play();
            await UniTask.WaitUntil(() => animationPlay.state != PlayState.Playing);

            this.isPlaying = false;
            this.SetInputLock(true);
        }

        private void SetInputLock(bool value)
        {
            if (!this.eventSystem)
            {
                Debug.LogWarning("EventSystem is not set, cannot lock input. In view:" + this.gameObject.name);
                return;
            }

            this.eventSystem.enabled = value;
        }
    }
}