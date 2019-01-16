// Copyright © 2018, Meta Company.  All rights reserved.
// 
// Redistribution and use of this software (the "Software") in binary form, without modification, is 
// permitted provided that the following conditions are met:
// 
// 1.      Redistributions of the unmodified Software in binary form must reproduce the above 
//         copyright notice, this list of conditions and the following disclaimer in the 
//         documentation and/or other materials provided with the distribution.
// 2.      The name of Meta Company (“Meta”) may not be used to endorse or promote products derived 
//         from this Software without specific prior written permission from Meta.
// 3.      LIMITATION TO META PLATFORM: Use of the Software is limited to use on or in connection 
//         with Meta-branded devices or Meta-branded software development kits.  For example, a bona 
//         fide recipient of the Software may incorporate an unmodified binary version of the 
//         Software into an application limited to use on or in connection with a Meta-branded 
//         device, while he or she may not incorporate an unmodified binary version of the Software 
//         into an application designed or offered for use on a non-Meta-branded device.
// 
// For the sake of clarity, the Software may not be redistributed under any circumstances in source 
// code form, or in the form of modified binary code – and nothing in this License shall be construed 
// to permit such redistribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL META COMPANY BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using Meta.Tween;
using System.Collections;
using UnityEngine;

namespace Meta.Mouse
{
    /// <summary>
    /// Class responsible to provide user feedback about the meta mouse. It handles animations, sounds, etc...
    /// </summary>
    [DisallowMultipleComponent]
    internal class MetaMouseMessage : MonoBehaviour, IMetaMouseFeedback
    {
        /// <summary>
        /// Class that encapsulate the animation config values for the MetaMouseFeedback
        /// </summary>
        [System.Serializable]
        internal class MouseAnimationParameters
        {
            /// <summary>
            ///The target scale factor of the cursor
            /// </summary>
            [SerializeField]
            private float _cursorTargetScaleFactor;

            /// <summary>
            /// Duration of the mouse scale animation
            /// </summary>
            [SerializeField]
            private float _cursorScaleDuration;

            /// <summary>
            /// Curve of the mouse scale animation
            /// </summary>
            [SerializeField]
            private CurveAsset _cursorScaleAnimationCurve;

            /// <summary>
            /// Mouse text fade animation duration
            /// </summary>
            [SerializeField]
            private float _textFadeDuration;

            /// <summary>
            /// Mouse text stay duration, before disappearing
            /// </summary>
            [SerializeField]
            private float _textStayDuration;

            /// <summary>
            /// Mouse text message to show in the animation
            /// </summary>
            [SerializeField]
            private string _message;

            /// <summary>
            /// Mouse audio clip to play during the animation
            /// </summary>
            [SerializeField]
            private AudioClip _audioClip;

            /// <summary>
            ///The target scale of the cursor
            /// </summary>
            public Vector3 CursorTargetScale { get; private set; }

            /// <summary>
            /// Duration of the mouse scale animation
            /// </summary>
            public float CursorScaleDuration
            {
                get { return _cursorScaleDuration; }
                set { _cursorScaleDuration = value; }
            }

            /// <summary>
            /// Curve of the mouse scale animation
            /// </summary>
            public CurveAsset CursorScaleAnimationCurve
            {
                get { return _cursorScaleAnimationCurve; }
                set { _cursorScaleAnimationCurve = value; }
            }

            /// <summary>
            /// Mouse text fade animation duration
            /// </summary>
            public float TextFadeDuration
            {
                get { return _textFadeDuration; }
                set { _textFadeDuration = value; }
            }

            /// <summary>
            /// Mouse text stay duration, before disappearing
            /// </summary>
            public float TextStayDuration
            {
                get { return _textStayDuration; }
                set { _textStayDuration = value; }
            }

            /// <summary>
            /// Mouse text message to show in the animation
            /// </summary>
            public string Message
            {
                get { return _message; }
                set { _message = value; }
            }

            /// <summary>
            /// Mouse audio clip to play during the animation
            /// </summary>
            public AudioClip AudioClip
            {
                get { return _audioClip; }
                set { _audioClip = value; }
            }

            public void SetCursorTargetScale(Vector3 regularScale)
            {
                CursorTargetScale = regularScale * _cursorTargetScaleFactor;
            }
        }

        /// <summary>
        /// Mouse cursor transform. Use it to change its properties during animations.
        /// </summary>
        [SerializeField]
        private Transform _cursor;
        /// <summary>
        /// Main text shown in the animaition. Use it to change its properties during animations.
        /// </summary>
        [SerializeField]
        private TextMesh _mainText;
        /// <summary>
        /// Text showing the state of the mouse during the animation. Use it to change its properties during animations.
        /// </summary>
        [SerializeField]
        private TextMesh _stateText;
        /// <summary>
        /// Audio source necessary to play audio clips during the animation.
        /// </summary>
        [SerializeField]
        private AudioSource _audioSource;
        /// <summary>
        /// Parameters of the animation we play when the mouse is turned on.
        /// </summary>
        [SerializeField]
        private MouseAnimationParameters _onAnimationParameters;
        /// <summary>
        /// Parameters of the animation we play when the mouse is turned off.
        /// </summary>
        [SerializeField]
        private MouseAnimationParameters _offAnimationParameters;

        /// <summary>
        /// Scale used to highLight the mouse during the animations.
        /// </summary>
        [SerializeField]
        private MouseAnimationParameters _highlightParameters;

        /// <summary>
        /// Mouse cursor transform. Use it to change its properties during animations.
        /// </summary>
        public Transform Cursor
        {
            get { return _cursor; }
            set { _cursor = value; }
        }

        /// <summary>
        /// Main text shown in the animaition. Use it to change its properties during animations.
        /// </summary>
        public TextMesh MainText
        {
            get { return _mainText; }
            set { _mainText = value; }
        }

        /// <summary>
        /// Text showing the state of the mouse during the animation. Use it to change its properties during animations.
        /// </summary>
        public TextMesh StateText
        {
            get { return _stateText; }
            set { _stateText = value; }
        }

        /// <summary>
        /// Audio source necessary to play audio clips during the animation.
        /// </summary>
        public AudioSource AudioSource
        {
            get { return _audioSource; }
            set { _audioSource = value; }
        }

        /// <summary>
        /// Parameters of the animation we play when the mouse is turned on.
        /// </summary>
        public MouseAnimationParameters OnAnimationParameters
        {
            get { return _onAnimationParameters; }
            set { _onAnimationParameters = value; }
        }

        /// <summary>
        /// Parameters of the animation we play when the mouse is turned off.
        /// </summary>
        public MouseAnimationParameters OffAnimationParameters
        {
            get { return _offAnimationParameters; }
            set { _offAnimationParameters = value; }
        }

        /// <summary>
        /// Whether a mouse animation is running.
        /// </summary>
        public bool Animating { get; private set; }

        /// <summary>
        /// Start the mouse configuration. This will set the initial state of the mouse, but with no animation.
        /// </summary>
        /// <param name="visible"></param>
        public void HandleStartMouse(bool visible)
        {
            Vector3 regularCursorScale = _cursor.localScale;

            _onAnimationParameters.SetCursorTargetScale(regularCursorScale);
            _offAnimationParameters.SetCursorTargetScale(regularCursorScale);
            _highlightParameters.SetCursorTargetScale(regularCursorScale);

            EnableText(false);
            _cursor.localScale = visible ? _onAnimationParameters.CursorTargetScale : OffAnimationParameters.CursorTargetScale;
        }

        /// <summary>
        /// Play the animations to show or hide the mouse.
        /// </summary>
        /// <param name="visible"></param>
        public void HandleShowMouse(bool visible)
        {
            StopAnimations();
            if (visible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        /// <summary>
        /// Enable or disable the MetaMouse feedback.
        /// </summary>
        /// <param name="enable"></param>
        public void EnableMetaMouseFeedback(bool enable)
        {
            EnableText(enable);
            if (!enable)
            {
                StopAnimations();
            }
        }

        /// <summary>
        /// Highlight the meta mouse.
        /// </summary>
        /// <param name="metaMouseOn"></param>
        public void Highlight(bool metaMouseOn)
        {
            if (!Animating)
            {
                StartCoroutine(metaMouseOn ? PlayHighlightAnimation() : PlayNoMouseHighlightAnimation());
            }
        }


        /// <summary>
        /// Enable or disable the main text and the state text.
        /// </summary>
        /// <param name="enable"></param>
        private void EnableText(bool enable)
        {
            _mainText.gameObject.SetActive(enable);
            _stateText.gameObject.SetActive(enable);
            if (!enable)
            {
                SetTextTransparency(_mainText, 0);
                SetTextTransparency(_stateText, 0);
            }
        }

        private void StopAnimations()
        {
            Animating = false;
            StopAllCoroutines();
        }

        /// <summary>
        /// Coroutine that plays the animation to show or hide the main and state texts.
        /// </summary>
        /// <param name="animationParameters"></param>
        /// <returns></returns>
        private IEnumerator ShowText(MouseAnimationParameters animationParameters)
        {
            float fadeTimeMult = 1 / animationParameters.TextFadeDuration;

            _stateText.text = animationParameters.Message;

            EnableText(true);
            StartCoroutine(TextMeshTweens.Fade(_mainText, 1f, fadeTimeMult, null, null));
            yield return StartCoroutine(TextMeshTweens.Fade(_stateText, 1f, fadeTimeMult, null, null));
            yield return new WaitForSeconds(animationParameters.TextStayDuration);
            StartCoroutine(TextMeshTweens.Fade(_mainText, 0f, fadeTimeMult, null, null));
            yield return StartCoroutine(TextMeshTweens.Fade(_stateText, 0f, fadeTimeMult, null, null));
            EnableText(false);
        }

        /// <summary>
        /// Play the animation to show the mouse
        /// </summary>
        private void Show()
        {
            StopAnimations();
            if (_onAnimationParameters.AudioClip != null)
            {
                _audioSource.PlayOneShot(_onAnimationParameters.AudioClip);
            }
            StartCoroutine(PlayShowAnimation());
        }

        /// <summary>
        /// Play the animation to hide the mouse
        /// </summary>
        private void Hide()
        {
            StopAnimations();
            if (_offAnimationParameters.AudioClip != null)
            {
                _audioSource.PlayOneShot(_offAnimationParameters.AudioClip);
            }
            StartCoroutine(PlayHideAnimation());
        }

        /// <summary>
        /// Coroutine that plays the animation to show the mouse
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlayShowAnimation()
        {
            Animating = true;
            StartCoroutine(TransformTweens.ToScale(_cursor, _onAnimationParameters.CursorTargetScale, 1 / _onAnimationParameters.CursorScaleDuration, _onAnimationParameters.CursorScaleAnimationCurve.Curve, null));
            yield return StartCoroutine(ShowText(_onAnimationParameters));
            Animating = false;
        }

        /// <summary>
        /// Coroutine that plays the animation to hide the mouse
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlayHideAnimation()
        {
            Animating = true;
            yield return StartCoroutine(ShowText(_offAnimationParameters));
            yield return StartCoroutine(TransformTweens.ToScale(_cursor, _offAnimationParameters.CursorTargetScale, 1 / _offAnimationParameters.CursorScaleDuration, _offAnimationParameters.CursorScaleAnimationCurve.Curve, null));
            Animating = false;
        }

        /// <summary>
        /// Coroutine that plays the highlight animation to show the mouse
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlayHighlightAnimation()
        {
            Animating = true;
            Vector3 currentScale = _cursor.localScale;
            yield return StartCoroutine(TransformTweens.ToScale(_cursor, _highlightParameters.CursorTargetScale, 1 / _highlightParameters.CursorScaleDuration, _onAnimationParameters.CursorScaleAnimationCurve.Curve, null));
            yield return new WaitForSeconds(_highlightParameters.TextStayDuration);
            yield return StartCoroutine(TransformTweens.ToScale(_cursor, currentScale, 1 / _highlightParameters.CursorScaleDuration, _onAnimationParameters.CursorScaleAnimationCurve.Curve, null));
            Animating = false;
        }

        /// <summary>
        /// Coroutine that plays the highlight animation when the meta mouse is off.
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlayNoMouseHighlightAnimation()
        {
            Animating = true;
            yield return StartCoroutine(ShowText(_highlightParameters));
            Animating = false;
        }

        /// <summary>
        /// Set a text transparency
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alpha"></param>
        private static void SetTextTransparency(TextMesh text, float alpha)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }
}
