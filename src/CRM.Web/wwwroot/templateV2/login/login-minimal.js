'use strict';

(function () {
    const parallaxElements = document.querySelectorAll('[data-depth]');
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const parallaxState = { currentX: 0, currentY: 0, targetX: 0, targetY: 0, raf: null, listenersAttached: false };

    const shouldDisableParallax = () => prefersReducedMotion.matches || window.innerWidth < 768 || !parallaxElements.length;

    const stepParallax = () => {
        parallaxState.currentX += (parallaxState.targetX - parallaxState.currentX) * 0.08;
        parallaxState.currentY += (parallaxState.targetY - parallaxState.currentY) * 0.08;

        parallaxElements.forEach((el) => {
            const depth = parseFloat(el.dataset.depth || '0');
            const range = parseFloat(el.dataset.range || '40');
            const offsetX = -parallaxState.currentX * depth * range;
            const offsetY = -parallaxState.currentY * depth * range;
            el.style.setProperty('--parallax-x', `${offsetX}px`);
            el.style.setProperty('--parallax-y', `${offsetY}px`);
        });

        parallaxState.raf = requestAnimationFrame(stepParallax);
    };

    const handlePointerMove = (event) => {
        parallaxState.targetX = event.clientX / window.innerWidth - 0.5;
        parallaxState.targetY = event.clientY / window.innerHeight - 0.5;
    };

    const resetParallax = () => {
        parallaxState.targetX = 0;
        parallaxState.targetY = 0;
    };

    const attachParallaxListeners = () => {
        if (parallaxState.listenersAttached) return;
        window.addEventListener('pointermove', handlePointerMove);
        window.addEventListener('pointerup', resetParallax);
        window.addEventListener('pointercancel', resetParallax);
        document.addEventListener('mouseleave', resetParallax);
        window.addEventListener('blur', resetParallax);
        parallaxState.listenersAttached = true;
    };

    const detachParallaxListeners = () => {
        if (!parallaxState.listenersAttached) return;
        window.removeEventListener('pointermove', handlePointerMove);
        window.removeEventListener('pointerup', resetParallax);
        window.removeEventListener('pointercancel', resetParallax);
        document.removeEventListener('mouseleave', resetParallax);
        window.removeEventListener('blur', resetParallax);
        parallaxState.listenersAttached = false;
    };

    const initParallax = () => {
        if (shouldDisableParallax()) {
            detachParallaxListeners();
            parallaxState.currentX = 0;
            parallaxState.currentY = 0;
            parallaxState.targetX = 0;
            parallaxState.targetY = 0;
            parallaxElements.forEach((el) => {
                el.style.setProperty('--parallax-x', '0px');
                el.style.setProperty('--parallax-y', '0px');
            });
            if (parallaxState.raf) {
                cancelAnimationFrame(parallaxState.raf);
                parallaxState.raf = null;
            }
            return;
        }

        if (!parallaxState.raf) {
            parallaxState.raf = requestAnimationFrame(stepParallax);
        }
        attachParallaxListeners();
    };

    initParallax();
    if (typeof prefersReducedMotion.addEventListener === 'function') {
        prefersReducedMotion.addEventListener('change', initParallax);
    } else if (typeof prefersReducedMotion.addListener === 'function') {
        prefersReducedMotion.addListener(initParallax);
    }

    window.addEventListener('resize', initParallax, { passive: true });

    const form = document.getElementById('minimal-login-form');
    if (!form) {
        return;
    }

    const emailField = document.getElementById('email');
    const passwordField = document.getElementById('password');
    const toggleBtn = document.querySelector('[data-toggle="password"]');
    const submitBtn = form.querySelector('.primary-button');
    const defaultButtonContent = submitBtn ? submitBtn.innerHTML : '';

    const validators = {
        email(value) {
            const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return emailPattern.test(value.trim());
        },
        password(value) {
            return value.trim().length > 0;
        }
    };

    const fieldConfigs = {
        email: {
            input: emailField,
            wrapper: emailField?.closest('.form-field'),
            control: form.querySelector('.form-control[data-field="email"]')
        },
        password: {
            input: passwordField,
            wrapper: passwordField?.closest('.form-field'),
            control: form.querySelector('.form-control[data-field="password"]')
        }
    };

    const updateFilledState = (field) => {
        const config = fieldConfigs[field];
        if (!config?.wrapper || !config.input) return;
        const hasValue = config.input.value.trim().length > 0;
        config.wrapper.classList.toggle('filled', hasValue);
    };

    const setControlValidity = (field, isValid) => {
        const config = fieldConfigs[field];
        if (!config?.control || !config.wrapper || !config.input) return;
        const hasValue = config.input.value.trim().length > 0;

        if (isValid && hasValue) {
            config.control.classList.add('valid');
            config.wrapper.classList.add('validated');
        } else {
            config.control.classList.remove('valid');
            config.wrapper.classList.remove('validated');
        }
    };

    const setFieldState = (field, valid) => {
        const control = form.querySelector(`.form-control[data-field="${field}"]`);
        const message = form.querySelector(`.error-message[data-error="${field}"]`);
        if (!control) return;
        const wrapper = control.closest('.form-field');

        if (valid) {
            control.classList.remove('error');
            message?.classList.remove('visible');
            wrapper?.classList.remove('has-error');
        } else {
            control.classList.remove('valid');
            control.classList.add('error');
            message?.classList.add('visible');
            wrapper?.classList.add('has-error');
            wrapper?.classList.remove('validated');
        }
    };

    const attachFieldInteractions = (field) => {
        const config = fieldConfigs[field];
        if (!config?.input || !config.wrapper) return;

        config.input.addEventListener('focus', () => {
            config.wrapper.classList.add('focused');
        });

        config.input.addEventListener('blur', () => {
            config.wrapper.classList.remove('focused');
            updateFilledState(field);
            const isValid = validators[field](config.input.value);
            setFieldState(field, isValid);
            setControlValidity(field, isValid);
        });

        config.input.addEventListener('input', () => {
            updateFilledState(field);
            const isValid = validators[field](config.input.value);
            setFieldState(field, isValid);
            setControlValidity(field, isValid);
        });
    };

    Object.keys(fieldConfigs).forEach((field) => {
        attachFieldInteractions(field);
        updateFilledState(field);
    });

    const syncAutofillState = () => {
        Object.keys(fieldConfigs).forEach((field) => {
            const config = fieldConfigs[field];
            if (!config?.input || !config.wrapper) return;
            updateFilledState(field);
            const value = config.input.value;
            const hasValue = value && value.trim().length > 0;
            if (!hasValue) {
                config.control?.classList.remove('valid', 'error');
                config.wrapper.classList.remove('validated', 'has-error');
                return;
            }

            const isValid = validators[field](value);
            if (isValid) {
                config.control?.classList.add('valid');
                config.control?.classList.remove('error');
                config.wrapper.classList.add('validated');
                config.wrapper.classList.remove('has-error');
            }
        });
    };

    syncAutofillState();
    window.addEventListener('pageshow', syncAutofillState);
    setTimeout(syncAutofillState, 200);

    const validate = () => {
        const emailValid = validators.email(emailField.value);
        const passwordValid = validators.password(passwordField.value);

        setFieldState('email', emailValid);
        setFieldState('password', passwordValid);
        setControlValidity('email', emailValid);
        setControlValidity('password', passwordValid);

        return emailValid && passwordValid;
    };

    toggleBtn?.addEventListener('click', () => {
        if (!passwordField) return;
        const isPassword = passwordField.type === 'password';
        passwordField.type = isPassword ? 'text' : 'password';
        toggleBtn.textContent = isPassword ? 'Gizle' : 'Göster';
    });

    form.addEventListener('submit', (event) => {
        const isValid = validate();

        if (!submitBtn) return;

        if (!isValid) {
            event.preventDefault();
            submitBtn.classList.remove('success');
            submitBtn.innerHTML = defaultButtonContent;
            return;
        }

        submitBtn.classList.add('success');
        submitBtn.innerHTML = "Giriş başarılı <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='1.5' stroke-linecap='round' stroke-linejoin='round' width='16' height='16'><path d='M5 13l4 4L19 7' /></svg>";
        submitBtn.setAttribute('disabled', 'disabled');
    });
})();
