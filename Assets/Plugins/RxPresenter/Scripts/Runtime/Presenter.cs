﻿using System;
using UniRx;
using UnityEngine;

namespace Boscohyun.RxPresenter
{
    [DisallowMultipleComponent]
    public class Presenter<T> : MonoBehaviour, IReactivePresenter<T>, IView, IViewAnimator
        where T : Presenter<T>
    {
        protected static readonly int AnimatorHashShow = Animator.StringToHash("Show");
        protected static readonly int AnimatorHashHide = Animator.StringToHash("Hide");

        private ReactivePresenter _reactivePresenter;

        [SerializeField] private Animator animator;
        [SerializeField] private bool animatorAlwaysEnabled;
        [SerializeField] private bool showAtAwake;

        #region IView

        bool IView.ActiveSelf => gameObject.activeSelf;

        bool IView.HasViewAnimator => animator;

        IViewAnimator IView.ViewAnimator => this;

        void IView.SetActive(bool active) => gameObject.SetActive(active);

        #endregion

        #region IViewAnimator

        bool IViewAnimator.AnimatorAlwaysActive => animatorAlwaysEnabled;

        ViewAnimatorState IViewAnimator.CurrentAnimatorState =>
            animator.GetCurrentAnimatorStateInfo(default).shortNameHash == AnimatorHashHide
                ? ViewAnimatorState.Hide
                : ViewAnimatorState.Show;

        float IViewAnimator.CurrentAnimatorStateNormalizedTime =>
            animator.GetCurrentAnimatorStateInfo(default).normalizedTime;
        
        void IViewAnimator.PlayAnimation(ViewAnimatorState viewAnimatorState, float normalizedTime) =>
            animator.Play(
                viewAnimatorState == ViewAnimatorState.Hide
                    ? AnimatorHashHide
                    : AnimatorHashShow,
                default,
                normalizedTime);

        void IViewAnimator.SetActive(ViewAnimatorState viewAnimatorState, bool active)
        {
            if (active == animator.enabled)
            {
                return;
            }
            
            animator.enabled = active;
        }

        #endregion

        public PresenterState PresenterState => _reactivePresenter.PresenterState;
        
        public IObservable<T> OnPresenterStateChange => _reactivePresenter.OnPresenterStateChange.Select(_ => (T) this);
        public IObservable<T> OnShowAnimationBeginning => _reactivePresenter.OnPresenterStateChange.Select(_ => (T) this);
        public IObservable<T> OnShowAnimationEnd => _reactivePresenter.OnPresenterStateChange.Select(_ => (T) this);
        public IObservable<T> OnHideAnimationBeginning => _reactivePresenter.OnPresenterStateChange.Select(_ => (T) this);
        public IObservable<T> OnHideAnimationEnd => _reactivePresenter.OnPresenterStateChange.Select(_ => (T) this);

        protected virtual void Awake()
        {
            _reactivePresenter = new ReactivePresenter(this);
            if (showAtAwake)
            {
                _reactivePresenter.Show();
            }
        }

        protected virtual void OnDestroy()
        {
            ((IDisposable) this).Dispose();
        }

        void IDisposable.Dispose()
        {
            _reactivePresenter.Dispose();
        }

        public void Show(bool skipAnimation = default) => _reactivePresenter.Show(skipAnimation);

        public void Show(Action<T> callback) => _reactivePresenter.Show(_ => callback?.Invoke((T) this));

        public void Show(bool skipAnimation, Action<T> callback) =>
            _reactivePresenter.Show(skipAnimation, _ => callback?.Invoke((T) this));

        public IObservable<T> ShowAsObservable(bool skipAnimation = default) =>
            _reactivePresenter.ShowAsObservable(skipAnimation).Select(_ => (T) this);

        public void Hide(bool skipAnimation = default) => _reactivePresenter.Hide(skipAnimation);

        public void Hide(Action<T> callback) => _reactivePresenter.Hide(_ => callback?.Invoke((T) this));

        public void Hide(bool skipAnimation, Action<T> callback) =>
            _reactivePresenter.Hide(skipAnimation, _ => callback?.Invoke((T) this));

        public IObservable<T> HideAsObservable(bool skipAnimation = default) =>
            _reactivePresenter.HideAsObservable(skipAnimation).Select(_ => (T) this);
    }
}
