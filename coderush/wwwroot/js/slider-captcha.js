/*
 * SliderCaptcha - Open-source image-based slider puzzle CAPTCHA
 * Inspired by AJ-Captcha (open-source, Douyin/ByteDance ecosystem)
 * Canvas-based puzzle with server-side HMAC token validation
 */
(function () {
    'use strict';

    var W = 280, H = 155, P = 42, TAB = 7;

    function SliderCaptcha(containerId, hiddenId, formId) {
        this.el = document.getElementById(containerId);
        this.hiddenId = hiddenId;
        this.formId = formId;
        this.verified = false;
        this.challengeToken = null;
        this.targetX = 0;
        this.pieceY = 0;
        this.startTime = 0;
        this.cleanBg = null;
        this._build();
        this._events();
    }

    /* ── DOM construction ── */
    SliderCaptcha.prototype._build = function () {
        var wrap = document.createElement('div');
        wrap.className = 'captcha-wrapper';

        // Checkbox row
        var row = document.createElement('div');
        row.className = 'captcha-checkbox-row';

        var cb = document.createElement('div');
        cb.className = 'captcha-checkbox';
        cb.innerHTML = '<div class="captcha-check-icon">&#10003;</div>';

        var lbl = document.createElement('span');
        lbl.className = 'captcha-label';
        lbl.innerHTML = 'I am not a robot<span class="captcha-label-sub">Slider CAPTCHA</span>';

        row.appendChild(cb);
        row.appendChild(lbl);
        wrap.appendChild(row);

        // Popup
        var popup = document.createElement('div');
        popup.className = 'captcha-popup';
        popup.style.display = 'none';

        // Canvas
        var cWrap = document.createElement('div');
        cWrap.className = 'captcha-canvas-wrap';

        var bgC = document.createElement('canvas');
        bgC.width = W; bgC.height = H;
        bgC.className = 'captcha-bg-canvas';

        var pcC = document.createElement('canvas');
        pcC.width = W; pcC.height = H;
        pcC.className = 'captcha-piece-canvas';

        cWrap.appendChild(bgC);
        cWrap.appendChild(pcC);
        popup.appendChild(cWrap);

        // Stats
        var stats = document.createElement('div');
        stats.className = 'captcha-stats';
        stats.textContent = 'Drag the slider to complete the puzzle';
        popup.appendChild(stats);

        // Slider track
        var track = document.createElement('div');
        track.className = 'captcha-slider-track';

        var fill = document.createElement('div');
        fill.className = 'captcha-slider-fill';

        var thumb = document.createElement('div');
        thumb.className = 'captcha-slider-thumb';
        thumb.innerHTML = '&#10148;';

        var hint = document.createElement('span');
        hint.className = 'captcha-slider-hint';
        hint.textContent = 'Slide to complete puzzle \u2192';

        track.appendChild(fill);
        track.appendChild(thumb);
        track.appendChild(hint);
        popup.appendChild(track);

        // Refresh
        var refresh = document.createElement('div');
        refresh.className = 'captcha-refresh';
        refresh.innerHTML = '&#x21bb; New puzzle';
        popup.appendChild(refresh);

        wrap.appendChild(popup);

        // Error message
        var errMsg = document.createElement('div');
        errMsg.className = 'captcha-error-msg';
        errMsg.textContent = 'Please complete the CAPTCHA verification.';
        wrap.appendChild(errMsg);

        this.el.appendChild(wrap);

        // Store refs
        this._row = row;
        this._cb = cb;
        this._popup = popup;
        this._bgC = bgC;
        this._pcC = pcC;
        this._stats = stats;
        this._fill = fill;
        this._thumb = thumb;
        this._hint = hint;
        this._refresh = refresh;
        this._errMsg = errMsg;

        // Clear any pre-existing token (force fresh solve on every page load)
        var hidden = document.getElementById(this.hiddenId);
        if (hidden) hidden.value = '';
    };

    /* ── Events ── */
    SliderCaptcha.prototype._events = function () {
        var self = this;

        // Checkbox click
        this._row.addEventListener('click', function () {
            if (!self.verified) {
                self._load();
            }
        });

        // Refresh
        this._refresh.addEventListener('click', function () {
            self._reset();
            self._load();
        });

        // Slider drag
        var dragging = false, startX = 0, curX = 0;

        function onStart(e) {
            if (self.verified) return;
            dragging = true;
            startX = e.touches ? e.touches[0].clientX : e.clientX;
            curX = 0;
            self.startTime = Date.now();
            self._hint.style.opacity = '0';
            e.preventDefault();
        }
        function onMove(e) {
            if (!dragging) return;
            var cx = e.touches ? e.touches[0].clientX : e.clientX;
            curX = Math.max(0, Math.min(cx - startX, W - P - TAB));
            self._thumb.style.left = curX + 'px';
            self._fill.style.width = (curX + 17) + 'px';
            self._drawPiece(curX);
            e.preventDefault();
        }
        function onEnd() {
            if (!dragging) return;
            dragging = false;
            var ms = Date.now() - self.startTime;
            self._verify(Math.round(curX), ms);
        }

        this._thumb.addEventListener('mousedown', onStart);
        this._thumb.addEventListener('touchstart', onStart, { passive: false });
        document.addEventListener('mousemove', onMove);
        document.addEventListener('touchmove', onMove, { passive: false });
        document.addEventListener('mouseup', onEnd);
        document.addEventListener('touchend', onEnd);

        // Form submit guard
        if (this.formId) {
            var form = document.getElementById(this.formId);
            if (form) {
                form.addEventListener('submit', function (e) {
                    if (!self.verified) {
                        e.preventDefault();
                        self._errMsg.classList.add('captcha-error-show');
                        self._row.style.borderColor = '#e53935';
                        setTimeout(function () {
                            self._row.style.borderColor = '';
                        }, 2000);
                    }
                });
            }
        }
    };

    /* ── Load challenge ── */
    SliderCaptcha.prototype._load = function () {
        var self = this;
        this._popup.style.display = 'block';
        this._popup.innerHTML = '<div class="captcha-loading">Loading puzzle\u2026</div>';

        fetch('/api/Captcha/Challenge')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                self.challengeToken = data.Token;
                self.targetX = data.PuzzleX;
                self.pieceY = 20 + Math.floor(Math.random() * (H - P - TAB - 40));
                self._rebuildPopup();
                self._drawBg();
                self._drawPiece(0);
            })
            .catch(function () {
                self._popup.innerHTML = '<div class="captcha-loading" style="color:#e53935">Failed to load. Click to retry.</div>';
                self._popup.onclick = function () { self._popup.onclick = null; self._load(); };
            });
    };

    /* ── Rebuild popup DOM after loading ── */
    SliderCaptcha.prototype._rebuildPopup = function () {
        this._popup.innerHTML = '';

        var cWrap = document.createElement('div');
        cWrap.className = 'captcha-canvas-wrap';

        var bgC = document.createElement('canvas');
        bgC.width = W; bgC.height = H;
        bgC.className = 'captcha-bg-canvas';

        var pcC = document.createElement('canvas');
        pcC.width = W; pcC.height = H;
        pcC.className = 'captcha-piece-canvas';

        cWrap.appendChild(bgC);
        cWrap.appendChild(pcC);
        this._popup.appendChild(cWrap);

        var stats = document.createElement('div');
        stats.className = 'captcha-stats';
        stats.textContent = 'Drag the slider to complete the puzzle';
        this._popup.appendChild(stats);

        var track = document.createElement('div');
        track.className = 'captcha-slider-track';

        var fill = document.createElement('div');
        fill.className = 'captcha-slider-fill';

        var thumb = document.createElement('div');
        thumb.className = 'captcha-slider-thumb';
        thumb.innerHTML = '&#10148;';

        var hint = document.createElement('span');
        hint.className = 'captcha-slider-hint';
        hint.textContent = 'Slide to complete puzzle \u2192';

        track.appendChild(fill);
        track.appendChild(thumb);
        track.appendChild(hint);
        this._popup.appendChild(track);

        var refresh = document.createElement('div');
        refresh.className = 'captcha-refresh';
        refresh.innerHTML = '&#x21bb; New puzzle';
        this._popup.appendChild(refresh);

        this._bgC = bgC;
        this._pcC = pcC;
        this._stats = stats;
        this._fill = fill;
        this._thumb = thumb;
        this._hint = hint;
        this._refresh = refresh;

        // Rebind events on new elements
        var self = this;
        this._refresh.addEventListener('click', function () {
            self._reset();
            self._load();
        });

        var dragging = false, startX = 0, curX = 0;

        function onStart(e) {
            if (self.verified) return;
            dragging = true;
            startX = e.touches ? e.touches[0].clientX : e.clientX;
            curX = 0;
            self.startTime = Date.now();
            self._hint.style.opacity = '0';
            e.preventDefault();
        }
        function onMove(e) {
            if (!dragging) return;
            var cx = e.touches ? e.touches[0].clientX : e.clientX;
            curX = Math.max(0, Math.min(cx - startX, W - P - TAB));
            self._thumb.style.left = curX + 'px';
            self._fill.style.width = (curX + 17) + 'px';
            self._drawPiece(curX);
            e.preventDefault();
        }
        function onEnd() {
            if (!dragging) return;
            dragging = false;
            var ms = Date.now() - self.startTime;
            self._verify(Math.round(curX), ms);
        }

        this._thumb.addEventListener('mousedown', onStart);
        this._thumb.addEventListener('touchstart', onStart, { passive: false });

        // Remove old document listeners & re-add
        this._docMove && document.removeEventListener('mousemove', this._docMove);
        this._docTouchMove && document.removeEventListener('touchmove', this._docTouchMove);
        this._docUp && document.removeEventListener('mouseup', this._docUp);
        this._docTouchEnd && document.removeEventListener('touchend', this._docTouchEnd);

        this._docMove = onMove;
        this._docTouchMove = onMove;
        this._docUp = onEnd;
        this._docTouchEnd = onEnd;

        document.addEventListener('mousemove', onMove);
        document.addEventListener('touchmove', onMove, { passive: false });
        document.addEventListener('mouseup', onEnd);
        document.addEventListener('touchend', onEnd);
    };

    /* ── Draw procedural background ── */
    SliderCaptcha.prototype._drawBg = function () {
        var ctx = this._bgC.getContext('2d');

        // Random hue for this challenge
        var hue = Math.floor(Math.random() * 360);

        // Base gradient
        var g = ctx.createLinearGradient(0, 0, W, H);
        g.addColorStop(0, 'hsl(' + hue + ',65%,55%)');
        g.addColorStop(0.5, 'hsl(' + ((hue + 50) % 360) + ',55%,45%)');
        g.addColorStop(1, 'hsl(' + ((hue + 110) % 360) + ',65%,40%)');
        ctx.fillStyle = g;
        ctx.fillRect(0, 0, W, H);

        // Random shapes
        var i;
        for (i = 0; i < 18; i++) {
            ctx.beginPath();
            var sh = (hue + Math.random() * 200) % 360;
            ctx.fillStyle = 'hsla(' + sh + ',55%,' + (25 + Math.random() * 45) + '%,0.25)';
            if (Math.random() > 0.5) {
                ctx.arc(Math.random() * W, Math.random() * H, 8 + Math.random() * 45, 0, Math.PI * 2);
            } else {
                ctx.rect(Math.random() * W, Math.random() * H, 15 + Math.random() * 65, 12 + Math.random() * 45);
            }
            ctx.fill();
        }

        // Bezier curves for texture
        for (i = 0; i < 6; i++) {
            ctx.beginPath();
            ctx.strokeStyle = 'hsla(' + ((hue + i * 40) % 360) + ',50%,70%,0.35)';
            ctx.lineWidth = 1 + Math.random() * 3;
            ctx.moveTo(Math.random() * W, Math.random() * H);
            ctx.bezierCurveTo(
                Math.random() * W, Math.random() * H,
                Math.random() * W, Math.random() * H,
                Math.random() * W, Math.random() * H
            );
            ctx.stroke();
        }

        // Grid dots for noise
        for (i = 0; i < 60; i++) {
            ctx.beginPath();
            ctx.fillStyle = 'hsla(0,0%,' + (Math.random() * 100) + '%,0.15)';
            ctx.arc(Math.random() * W, Math.random() * H, 1 + Math.random() * 2, 0, Math.PI * 2);
            ctx.fill();
        }

        // Save clean background (before hole)
        this.cleanBg = document.createElement('canvas');
        this.cleanBg.width = W;
        this.cleanBg.height = H;
        this.cleanBg.getContext('2d').drawImage(this._bgC, 0, 0);

        // Draw the puzzle hole
        this._piecePath(ctx, this.targetX, this.pieceY);
        ctx.fillStyle = 'rgba(0,0,0,0.45)';
        ctx.fill();
        ctx.strokeStyle = 'rgba(255,255,255,0.6)';
        ctx.lineWidth = 1.5;
        ctx.stroke();
    };

    /* ── Puzzle piece path (jigsaw shape) ── */
    SliderCaptcha.prototype._piecePath = function (ctx, x, y) {
        var s = P, r = TAB;
        ctx.beginPath();
        ctx.moveTo(x, y);
        // Top with tab
        ctx.lineTo(x + s * 0.38, y);
        ctx.arc(x + s * 0.5, y, r, Math.PI, 0, false);
        ctx.lineTo(x + s, y);
        // Right with tab
        ctx.lineTo(x + s, y + s * 0.38);
        ctx.arc(x + s, y + s * 0.5, r, -Math.PI / 2, Math.PI / 2, false);
        ctx.lineTo(x + s, y + s);
        // Bottom
        ctx.lineTo(x, y + s);
        // Left
        ctx.lineTo(x, y);
        ctx.closePath();
    };

    /* ── Draw movable piece at slider offset ── */
    SliderCaptcha.prototype._drawPiece = function (offsetX) {
        var ctx = this._pcC.getContext('2d');
        ctx.clearRect(0, 0, W, H);
        if (!this.cleanBg) return;

        // Clip to piece shape at offsetX and draw the original background shifted
        ctx.save();
        this._piecePath(ctx, offsetX, this.pieceY);
        ctx.clip();
        ctx.drawImage(this.cleanBg, offsetX - this.targetX, 0);
        ctx.restore();

        // Piece border
        this._piecePath(ctx, offsetX, this.pieceY);
        ctx.strokeStyle = 'rgba(255,255,255,0.85)';
        ctx.lineWidth = 2;
        ctx.stroke();

        // Drop shadow effect
        ctx.save();
        ctx.shadowColor = 'rgba(0,0,0,0.3)';
        ctx.shadowBlur = 6;
        ctx.shadowOffsetX = 2;
        ctx.shadowOffsetY = 2;
        this._piecePath(ctx, offsetX, this.pieceY);
        ctx.strokeStyle = 'rgba(0,0,0,0.01)';
        ctx.stroke();
        ctx.restore();
    };

    /* ── Verify with server ── */
    SliderCaptcha.prototype._verify = function (userX, ms) {
        var self = this;

        this._stats.className = 'captcha-stats';
        this._stats.textContent = 'Verifying... (' + (ms / 1000).toFixed(2) + 's)';

        fetch('/api/Captcha/Verify', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Token: self.challengeToken,
                UserX: userX,
                SolveTimeMs: ms
            })
        })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            if (data.Success) {
                self._pass(data.VerificationToken, ms);
            } else {
                self._fail();
            }
        })
        .catch(function () {
            self._fail();
        });
    };

    /* ── Success ── */
    SliderCaptcha.prototype._pass = function (token, ms) {
        this.verified = true;

        // Set hidden form field
        var hidden = document.getElementById(this.hiddenId);
        if (hidden) hidden.value = token;

        // Update stats
        this._stats.className = 'captcha-stats captcha-stats-ok';
        this._stats.textContent = '\u2713 Verified in ' + (ms / 1000).toFixed(2) + 's';

        // Animate slider
        this._thumb.classList.add('captcha-thumb-ok');
        this._thumb.innerHTML = '&#10003;';
        this._fill.classList.add('captcha-fill-ok');

        // Collapse popup after delay
        var self = this;
        setTimeout(function () {
            self._popup.style.display = 'none';
            self._cb.classList.add('captcha-checked');
            self._errMsg.classList.remove('captcha-error-show');
            self._row.style.borderColor = '#4caf50';
        }, 800);
    };

    /* ── Failure ── */
    SliderCaptcha.prototype._fail = function () {
        var self = this;

        this._stats.className = 'captcha-stats captcha-stats-fail';
        this._stats.textContent = '\u2717 Verification failed \u2014 try again';
        this._thumb.classList.add('captcha-thumb-fail');
        this._fill.classList.add('captcha-fill-fail');

        setTimeout(function () {
            self._reset();
            self._load();
        }, 1200);
    };

    /* ── Reset state ── */
    SliderCaptcha.prototype._reset = function () {
        this.verified = false;
        this.challengeToken = null;
        this.cleanBg = null;
        var hidden = document.getElementById(this.hiddenId);
        if (hidden) hidden.value = '';
        this._cb.classList.remove('captcha-checked');
        this._row.style.borderColor = '';
    };

    window.SliderCaptcha = SliderCaptcha;
})();
