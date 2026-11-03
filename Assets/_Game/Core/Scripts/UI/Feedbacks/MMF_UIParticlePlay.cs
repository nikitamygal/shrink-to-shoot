using System;
using Coffee.UIExtensions;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Game.UI.Feedbacks
{
    [Serializable]
    [FeedbackPath("UI/UIParticle Play")]
    [FeedbackHelp("Restarts a UIParticle group and places it at the position supplied to the feedback player.")]
    public class MMF_UIParticlePlay : MMF_Feedback
    {
        [MMFInspectorGroup("UIParticle", true, 41, true)]
        [Tooltip("UI particle group to restart when this feedback plays.")]
        public UIParticle Target;
        [Tooltip("Move the whole UI particle group to the position passed to MMF_Player.")]
        public bool MoveToPosition = true;

#if UNITY_EDITOR
        public override bool EvaluateRequiresSetup() => Target == null;
        public override string RequiresSetupText => "Assign the UIParticle group to play.";
        public override string RequiredTargetText => Target != null ? Target.name : string.Empty;
#endif

        protected override void CustomInitialization(MMF_Player owner)
        {
            base.CustomInitialization(owner);
            if (Active) StopParticles();
        }

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1f)
        {
            if (!Active || Target == null) return;
            StopParticles();
            if (MoveToPosition) Target.transform.position = position;
            Target.Play();
            DisablePlayOnAwake();
        }

        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1f)
        {
            if (Active) StopParticles();
        }

        protected override void CustomReset()
        {
            base.CustomReset();
            if (Active) StopParticles();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            StopParticles();
        }

        private void StopParticles()
        {
            if (Target == null) return;
            Target.Stop();
            Target.Clear();
            DisablePlayOnAwake();
        }

        private void DisablePlayOnAwake()
        {
            if (Target == null) return;
            foreach (var system in Target.particles)
            {
                if (system == null) continue;
                var main = system.main;
                main.playOnAwake = false;
            }
        }
    }
}
