(function () {
    const toggle = document.querySelector("[data-nav-toggle]");
    const nav = document.querySelector("[data-nav]");

    if (toggle && nav) {
        const setOpen = (open) => {
            nav.classList.toggle("is-open", open);
            toggle.setAttribute("aria-expanded", open ? "true" : "false");
            toggle.textContent = open ? "Close" : "Menu";
            document.body.classList.toggle("nav-open", open);
        };

        toggle.addEventListener("click", () => setOpen(!nav.classList.contains("is-open")));
        nav.querySelectorAll("a").forEach((link) => link.addEventListener("click", () => setOpen(false)));
        window.addEventListener("resize", () => {
            if (window.innerWidth > 1080) setOpen(false);
        });
    }

    const themeToggle = document.querySelector("[data-theme-toggle]");
    if (themeToggle) {
        const themeColor = document.querySelector('meta[name="theme-color"]');
        const applyTheme = (theme) => {
            document.documentElement.setAttribute("data-theme", theme);
            const label = theme === "dark" ? "Switch to light mode" : "Switch to dark mode";
            themeToggle.setAttribute("aria-label", label);
            themeToggle.title = label;
            themeColor?.setAttribute("content", theme === "dark" ? "#14110e" : "#f6f1e8");
        };

        applyTheme(document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light");
        themeToggle.addEventListener("click", () => {
            const next = document.documentElement.getAttribute("data-theme") === "dark" ? "light" : "dark";
            applyTheme(next);
            try { localStorage.setItem("theme", next); } catch { }
        });
    }

    const hscroll = document.querySelector("[data-hscroll]");
    if (hscroll) {
        const pin = hscroll.querySelector(".hscroll-pin");
        const viewport = hscroll.querySelector("[data-hscroll-viewport]");
        const track = hscroll.querySelector("[data-hscroll-track]");
        const bar = hscroll.querySelector("[data-hscroll-bar]");
        const counter = hscroll.querySelector("[data-hscroll-current]");
        const cards = [...track.querySelectorAll(".collection-card")];
        const header = document.querySelector(".site-header");
        const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
        const shortScreen = window.matchMedia("(max-height: 480px)");
        // Vertical pixels scrolled per horizontal pixel moved; above 1 keeps the travel unhurried.
        const scrollRatio = 1.2;
        const smoothing = 0.14;
        const clamp = (value, min, max) => Math.min(Math.max(value, min), max);

        let pinned = false;
        let measured = false;
        let distance = 0;
        let travel = 0;
        let viewWidth = 0;
        let centers = [];
        let current = 0;
        let target = 0;
        let frame = 0;
        let lastTime = 0;
        let shown = -1;

        const paint = (x) => {
            bar.style.transform = `scaleX(${distance > 0 ? x / distance : 0})`;
            const middle = x + viewWidth / 2;
            let index = 0;
            centers.forEach((center, i) => {
                if (Math.abs(center - middle) < Math.abs(centers[index] - middle)) index = i;
            });
            if (index !== shown) {
                shown = index;
                counter.textContent = String(index + 1).padStart(2, "0");
            }
        };

        const readTarget = () => travel > 0
            ? clamp(-hscroll.getBoundingClientRect().top / travel, 0, 1) * distance
            : 0;

        const tick = (now) => {
            frame = 0;
            target = readTarget();
            const elapsed = lastTime ? Math.min(now - lastTime, 64) : 16.7;
            lastTime = now;
            current += (target - current) * (1 - Math.pow(1 - smoothing, elapsed / 16.7));
            if (Math.abs(target - current) < 0.25) current = target;

            track.style.transform = `translate3d(${(-current).toFixed(2)}px, 0, 0)`;
            paint(current);

            if (current !== target) frame = requestAnimationFrame(tick);
            else lastTime = 0;
        };

        const request = () => {
            if (pinned && !frame) frame = requestAnimationFrame(tick);
        };

        const measure = () => {
            if (header) document.documentElement.style.setProperty("--header-h", `${header.offsetHeight}px`);
            viewWidth = viewport.clientWidth;
            distance = Math.max(0, track.offsetWidth - viewWidth);
            centers = cards.map((card) => card.offsetLeft + card.offsetWidth / 2);
            pinned = distance > 0 && !reduceMotion.matches && !shortScreen.matches;
            hscroll.classList.toggle("is-pinned", pinned);

            if (!pinned) {
                cancelAnimationFrame(frame);
                frame = 0;
                hscroll.style.height = "";
                track.style.transform = "";
                current = target = 0;
                distance = Math.max(0, viewport.scrollWidth - viewWidth);
                paint(viewport.scrollLeft);
                return;
            }

            viewport.scrollLeft = 0;
            travel = distance * scrollRatio;
            hscroll.style.height = `${pin.offsetHeight + travel}px`;
            target = readTarget();
            current = measured ? Math.min(current, distance) : target;
            measured = true;
            track.style.transform = `translate3d(${(-current).toFixed(2)}px, 0, 0)`;
            paint(current);
            request();
        };

        let resizeFrame = 0;
        const scheduleMeasure = () => {
            cancelAnimationFrame(resizeFrame);
            resizeFrame = requestAnimationFrame(measure);
        };

        window.addEventListener("scroll", request, { passive: true });
        window.addEventListener("resize", scheduleMeasure);
        window.addEventListener("load", scheduleMeasure);
        document.fonts?.ready.then(scheduleMeasure);
        if ("ResizeObserver" in window) new ResizeObserver(scheduleMeasure).observe(track);
        reduceMotion.addEventListener?.("change", scheduleMeasure);
        shortScreen.addEventListener?.("change", scheduleMeasure);

        viewport.addEventListener("scroll", () => {
            if (!pinned) paint(viewport.scrollLeft);
        }, { passive: true });

        track.addEventListener("focusin", (event) => {
            if (!pinned || distance <= 0 || !event.target.matches(":focus-visible")) return;
            const item = event.target.closest(".collection-card, .hscroll-end");
            if (!item) return;
            pin.scrollLeft = 0;
            viewport.scrollLeft = 0;
            const x = clamp(item.offsetLeft - (viewWidth - item.offsetWidth) / 2, 0, distance);
            const start = hscroll.getBoundingClientRect().top + window.scrollY;
            window.scrollTo({ top: start + (x / distance) * travel, behavior: "instant" });
        });

        measure();
    }

    document.querySelectorAll("form[data-confirm]").forEach((form) => {
        form.addEventListener("submit", (event) => {
            if (!window.confirm(form.dataset.confirm || "Are you sure?"))
                event.preventDefault();
        });
    });

    const gallery = document.querySelector("[data-detail-gallery]");
    if (gallery) {
        const main = gallery.querySelector("[data-detail-main]");
        const buttons = [...gallery.querySelectorAll("[data-src]")];
        buttons.forEach((button) => {
            button.addEventListener("click", () => {
                if (!main) return;
                main.src = button.dataset.src;
                if (button.dataset.alt) main.alt = button.dataset.alt;
                buttons.forEach((item) => item.classList.toggle("is-active", item === button));
            });
        });
    }

    const grid = document.querySelector("[data-gallery-grid]");
    if (grid) {
        const items = [...grid.querySelectorAll("[data-category]")];
        const buttons = [...document.querySelectorAll("[data-filter]")];
        buttons.forEach((button) => {
            button.addEventListener("click", () => {
                const filter = button.dataset.filter;
                buttons.forEach((item) => {
                    const active = item === button;
                    item.classList.toggle("is-active", active);
                    item.setAttribute("aria-pressed", active ? "true" : "false");
                });
                items.forEach((item) => {
                    item.hidden = filter !== "all" && item.dataset.category !== filter;
                });
            });
        });
    }

    const box = document.querySelector("[data-lightbox-root]");
    const stage = box?.querySelector("[data-lightbox-stage]");
    if (box && stage) {
        const image = stage.querySelector("img");
        const caption = box.querySelector("[data-lightbox-caption]");
        const hint = box.querySelector("[data-lightbox-hint]");
        const closeButton = box.querySelector("[data-lightbox-close]");
        const zoomInButton = box.querySelector("[data-zoom-in]");
        const zoomOutButton = box.querySelector("[data-zoom-out]");
        const fitButton = box.querySelector("[data-zoom-fit]");
        const filmButton = box.querySelector("[data-film]");
        const video = box.querySelector("[data-lightbox-video]");
        const progress = box.querySelector("[data-film-progress]");
        const finePointer = window.matchMedia("(hover: hover) and (pointer: fine)");
        const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
        const clamp = (value, min, max) => Math.min(Math.max(value, min), max);
        const lerp = (a, b, t) => a + (b - a) * t;

        // scale 1 shows the whole photo fitted in the stage. The image is laid out at its largest
        // size and scaled down, so it is always drawn from full resolution and stays sharp.
        const view = { stageW: 0, stageH: 0, fitW: 0, fitH: 0, max: 3, scale: 1, x: 0, y: 0 };
        const pointers = new Map();
        let videoSrc = null;
        let modelSrc = null;
        let modelViewer = null;
        let modelLibrary = null;
        let opener = null;
        let lastTap = { time: 0, x: 0, y: 0 };
        let moved = false;
        let swallowRelease = false;
        let film = null;

        const manualHint = () => finePointer.matches
            ? "Click to zoom · move the mouse to explore · scroll to zoom in or out"
            : "Double-tap or pinch to zoom · drag to explore";

        const apply = (eased) => {
            stage.classList.toggle("is-easing", eased);
            stage.classList.toggle("is-zoomed", view.scale > 1.01);
            image.style.transform = `translate3d(${view.x.toFixed(1)}px, ${view.y.toFixed(1)}px, 0) scale(${(view.scale / view.max).toFixed(5)})`;
            zoomInButton.disabled = view.scale >= view.max - 0.01;
            zoomOutButton.disabled = view.scale <= 1.01;
        };

        const keepInside = () => {
            const width = view.fitW * view.scale;
            const height = view.fitH * view.scale;
            view.x = width <= view.stageW ? (view.stageW - width) / 2 : clamp(view.x, view.stageW - width, 0);
            view.y = height <= view.stageH ? (view.stageH - height) / 2 : clamp(view.y, view.stageH - height, 0);
        };

        const measure = () => {
            const rect = stage.getBoundingClientRect();
            const naturalW = image.naturalWidth || 1;
            const naturalH = image.naturalHeight || 1;
            const fit = Math.min(rect.width / naturalW, rect.height / naturalH);
            const ratio = view.scale;
            view.stageW = rect.width;
            view.stageH = rect.height;
            view.fitW = naturalW * fit;
            view.fitH = naturalH * fit;
            view.max = clamp((naturalW / view.fitW) * 2, 2.5, 6);
            view.scale = clamp(ratio, 1, view.max);
            image.style.width = `${view.fitW * view.max}px`;
            image.style.height = `${view.fitH * view.max}px`;
        };

        const ease = {
            sine: (t) => -(Math.cos(Math.PI * t) - 1) / 2
        };

        // The crystals are where near-white highlights change sharply from pixel to pixel; brass
        // and candles are too warm and too smooth to count. Returns the busiest patch, and a second
        // patch close by for the camera to drift to, as fractions of the photo's size.
        const findCrystals = () => {
            const fallback = [{ x: 0.5, y: 0.62 }, { x: 0.4, y: 0.66 }];
            try {
                const w = 96;
                const h = Math.max(8, Math.round((w * image.naturalHeight) / image.naturalWidth));
                const canvas = document.createElement("canvas");
                canvas.width = w;
                canvas.height = h;
                const context = canvas.getContext("2d", { willReadFrequently: true });
                context.drawImage(image, 0, 0, w, h);
                const data = context.getImageData(0, 0, w, h).data;
                const light = new Float32Array(w * h);
                for (let i = 0; i < w * h; i++) {
                    light[i] = data[i * 4 + 3] > 128
                        ? 0.3 * data[i * 4] + 0.59 * data[i * 4 + 1] + 0.11 * data[i * 4 + 2]
                        : 0;
                }
                const sparkle = new Float32Array(w * h);
                for (let y = 1; y < h - 1; y++) {
                    for (let x = 1; x < w - 1; x++) {
                        const i = y * w + x;
                        const white = Math.min(data[i * 4], data[i * 4 + 1], data[i * 4 + 2]);
                        if (data[i * 4 + 3] <= 128 || white < 170) continue;
                        sparkle[i] = Math.abs(light[i] - light[i - 1]) + Math.abs(light[i] - light[i + 1])
                            + Math.abs(light[i] - light[i - w]) + Math.abs(light[i] - light[i + w]);
                    }
                }

                const reach = Math.max(2, Math.round(w / 12));
                const scores = [];
                for (let y = reach; y < h - reach; y++) {
                    for (let x = reach; x < w - reach; x++) {
                        let score = 0;
                        for (let dy = -reach; dy <= reach; dy++) {
                            for (let dx = -reach; dx <= reach; dx++) score += sparkle[(y + dy) * w + x + dx];
                        }
                        scores.push({ x, y, score });
                    }
                }
                scores.sort((a, b) => b.score - a.score);
                const best = scores[0];
                if (!best || best.score <= 0) return fallback;
                const second = scores.find((spot) => {
                    const apart = Math.hypot(spot.x - best.x, spot.y - best.y);
                    return apart >= reach * 2 && apart <= reach * 5;
                }) ?? best;
                const at = (spot) => ({ x: (spot.x + 0.5) / w, y: (spot.y + 0.5) / h });
                return [at(best), at(second)];
            } catch {
                // Photos from another site cannot be read; the film then looks at the lower middle.
                return fallback;
            }
        };

        // The film: the whole piece → the camera moves in close on the crystals → drifts across
        // them → pulls back out to the whole piece, where the visitor takes over.
        const filmLength = 10.2;
        const filmShots = () => {
            const [first, second] = findCrystals();
            const deep = clamp(image.naturalWidth / view.fitW, 2.4, view.max);
            const whole = { x: 0.5, y: 0.5, scale: 1 };
            return [
                { at: 0, ...whole },
                { at: 0.6, ...whole },
                { at: 3.6, ...first, scale: deep },
                { at: 6.8, ...second, scale: Math.min(deep * 1.2, view.max) },
                { at: 9.8, ...whole }
            ];
        };

        const renderFilm = (time) => {
            const { shots } = film;
            const next = shots.findIndex((shot) => shot.at > time);
            const to = shots[next === -1 ? shots.length - 1 : next];
            const from = shots[next === -1 ? shots.length - 1 : next - 1];
            const t = to === from ? 1 : ease.sine((time - from.at) / (to.at - from.at));
            view.scale = Math.exp(lerp(Math.log(from.scale), Math.log(to.scale), t));
            view.x = view.stageW / 2 - lerp(from.x, to.x, t) * view.fitW * view.scale;
            view.y = view.stageH / 2 - lerp(from.y, to.y, t) * view.fitH * view.scale;
            keepInside();
            apply(false);
            progress.style.transform = `scaleX(${(time / filmLength).toFixed(4)})`;
        };

        const filmLabel = () => (modelSrc ? "▶ 3D model" : videoSrc ? "▶ 360° video" : "▶ Film");

        const stopFilm = () => {
            if (!film) return;
            cancelAnimationFrame(film.frame);
            if (film.media === "video") {
                video.pause();
                video.hidden = true;
            }
            if (film.media === "model") modelViewer.hidden = true;
            film = null;
            stage.classList.remove("is-film", "is-video", "is-model");
            filmButton.textContent = filmLabel();
            hint.textContent = manualHint();
            progress.style.transform = "scaleX(0)";
        };

        // A real video uploaded for the product plays like a normal video, with its own controls.
        const playVideo = () => {
            stopFilm();
            film = { media: "video", frame: 0 };
            stage.classList.add("is-video");
            video.hidden = false;
            if (video.getAttribute("src") !== videoSrc) video.src = videoSrc;
            video.currentTime = 0;
            video.play().catch(() => {});
            filmButton.textContent = "Photo";
            hint.textContent = "Press Photo to zoom into the details";
        };

        // An uploaded 3D model turns by itself all the way round; the visitor can drag it to any
        // side and zoom in. The viewer library is only downloaded the first time it is needed.
        const playModel = () => {
            stopFilm();
            film = { media: "model", frame: 0 };
            stage.classList.add("is-model");
            if (!modelViewer) {
                modelViewer = document.createElement("model-viewer");
                modelViewer.className = "lightbox-model";
                const settings = {
                    "camera-controls": "",
                    "auto-rotate": "",
                    "auto-rotate-delay": "0",
                    "rotation-per-second": "30deg",
                    "interaction-prompt": "none",
                    "touch-action": "none",
                    "shadow-intensity": "1",
                    "environment-image": "neutral",
                    exposure: "1.1"
                };
                Object.entries(settings).forEach(([name, value]) => modelViewer.setAttribute(name, value));
                stage.insertBefore(modelViewer, progress);
            }
            modelViewer.hidden = false;
            modelViewer.setAttribute("alt", caption.textContent || "3D model");
            if (modelViewer.getAttribute("src") !== modelSrc) modelViewer.setAttribute("src", modelSrc);
            modelLibrary ??= import("/lib/model-viewer/model-viewer.min.js");
            modelLibrary.catch(() => {
                modelLibrary = null;
                if (film?.media === "model") hint.textContent = "The 3D model could not be loaded · press Photo";
            });
            filmButton.textContent = "Photo";
            hint.textContent = finePointer.matches
                ? "Drag to turn it · scroll to zoom · press Photo for the photo"
                : "Drag to turn it · pinch to zoom · press Photo for the photo";
        };

        const playFilm = () => {
            if (modelSrc) {
                playModel();
                return;
            }
            if (videoSrc) {
                playVideo();
                return;
            }
            if (!image.naturalWidth) return;
            stopFilm();
            film = { start: performance.now(), frame: 0, shots: filmShots() };
            stage.classList.add("is-film");
            filmButton.textContent = "Skip";
            hint.textContent = finePointer.matches ? "Click anywhere to stop and explore" : "Tap to stop and explore";
            renderFilm(0);
            const step = (now) => {
                if (!film) return;
                const time = (now - film.start) / 1000;
                renderFilm(Math.min(time, filmLength));
                if (time >= filmLength) stopFilm();
                else film.frame = requestAnimationFrame(step);
            };
            film.frame = requestAnimationFrame(step);
        };

        // Keeps the photo point under (px, py) in place while the scale changes.
        const zoomAt = (scale, px, py, eased) => {
            const next = clamp(scale, 1, view.max);
            const u = (px - view.x) / view.scale;
            const v = (py - view.y) / view.scale;
            view.scale = next;
            view.x = px - u * next;
            view.y = py - v * next;
            keepInside();
            apply(eased);
        };

        // With a mouse, the pointer position chooses which part of the zoomed photo is shown.
        const follow = (px, py, eased = true) => {
            const width = view.fitW * view.scale;
            const height = view.fitH * view.scale;
            view.x = (view.stageW - width) * clamp(px / view.stageW, 0, 1);
            view.y = (view.stageH - height) * clamp(py / view.stageH, 0, 1);
            keepInside();
            apply(eased);
        };

        const local = (event) => {
            const rect = stage.getBoundingClientRect();
            return { x: event.clientX - rect.left, y: event.clientY - rect.top };
        };

        const detailZoom = () => clamp(image.naturalWidth / view.fitW, 2, view.max);

        const toggleAt = (point, useFollow) => {
            if (view.scale > 1.01) {
                view.scale = 1;
                keepInside();
                apply(true);
            } else if (useFollow) {
                view.scale = detailZoom();
                follow(point.x, point.y);
            } else {
                zoomAt(detailZoom(), point.x, point.y, true);
            }
        };

        const open = (src, title, trigger, withFilm = false, videoSource = null, modelSource = null) => {
            stopFilm();
            opener = trigger;
            videoSrc = videoSource || null;
            modelSrc = modelSource || null;
            caption.textContent = title || "";
            image.alt = title || "Photograph";
            hint.textContent = manualHint();
            filmButton.hidden = !videoSrc && !modelSrc && reduceMotion.matches;
            filmButton.textContent = filmLabel();
            view.scale = 1;
            image.style.opacity = "0";
            box.hidden = false;
            document.body.classList.add("lightbox-open");
            let shown = false;
            const show = () => {
                if (shown) return;
                shown = true;
                measure();
                keepInside();
                apply(false);
                image.style.opacity = "1";
                if (withFilm && (videoSrc || modelSrc || !reduceMotion.matches)) playFilm();
            };
            image.onload = show;
            image.src = src;
            if (image.complete && image.naturalWidth) show();
            closeButton.focus();
        };

        const close = () => {
            stopFilm();
            box.hidden = true;
            document.body.classList.remove("lightbox-open");
            image.onload = null;
            image.removeAttribute("src");
            pointers.clear();
            opener?.focus({ preventScroll: true });
        };

        document.addEventListener("click", (event) => {
            if (!box.hidden) return;
            const link = event.target.closest("[data-lightbox]");
            const media = event.target.closest("[data-zoom-src]");
            const main = event.target.closest("[data-detail-main]");
            if (link) {
                event.preventDefault();
                open(link.getAttribute("href"), link.dataset.title, link);
            } else if (media) {
                event.preventDefault();
                open(media.dataset.zoomSrc, media.dataset.zoomTitle, media.closest("a") || media);
            } else if (main) {
                open(main.currentSrc || main.src, main.dataset.zoomTitle, main, true, main.dataset.videoSrc, main.dataset.modelSrc);
            }
        });

        box.addEventListener("click", (event) => {
            if (event.target === box || event.target.closest("[data-lightbox-close]")) close();
        });

        zoomInButton.addEventListener("click", () => {
            stopFilm();
            zoomAt(view.scale * 1.6, view.stageW / 2, view.stageH / 2, true);
        });
        zoomOutButton.addEventListener("click", () => {
            stopFilm();
            zoomAt(view.scale / 1.6, view.stageW / 2, view.stageH / 2, true);
        });
        fitButton.addEventListener("click", () => {
            stopFilm();
            view.scale = 1;
            keepInside();
            apply(true);
        });
        filmButton.addEventListener("click", () => (film ? stopFilm() : playFilm()));

        stage.addEventListener("wheel", (event) => {
            event.preventDefault();
            if (film?.media) return;
            stopFilm();
            const point = local(event);
            const next = view.scale * Math.exp(-event.deltaY * 0.0018);
            if (finePointer.matches) {
                view.scale = clamp(next, 1, view.max);
                follow(point.x, point.y);
            } else {
                zoomAt(next, point.x, point.y, false);
            }
        }, { passive: false });

        stage.addEventListener("pointerdown", (event) => {
            if (film?.media) return;
            if (film) {
                stopFilm();
                swallowRelease = true;
            }
            pointers.set(event.pointerId, local(event));
            moved = false;
            if (event.pointerType !== "mouse") stage.setPointerCapture(event.pointerId);
        });

        stage.addEventListener("pointermove", (event) => {
            if (film) return;
            const point = local(event);
            if (event.pointerType === "mouse") {
                if (view.scale > 1.01) follow(point.x, point.y);
                return;
            }
            const previous = pointers.get(event.pointerId);
            if (!previous) return;

            if (pointers.size === 2) {
                const [a, b] = [...pointers.values()];
                const before = Math.hypot(a.x - b.x, a.y - b.y);
                pointers.set(event.pointerId, point);
                const [c, d] = [...pointers.values()];
                const after = Math.hypot(c.x - d.x, c.y - d.y);
                if (before > 0) zoomAt(view.scale * (after / before), (c.x + d.x) / 2, (c.y + d.y) / 2, false);
                moved = true;
                return;
            }

            pointers.set(event.pointerId, point);
            if (Math.hypot(point.x - previous.x, point.y - previous.y) > 0) {
                view.x += point.x - previous.x;
                view.y += point.y - previous.y;
                keepInside();
                apply(false);
                moved = moved || view.scale > 1.01;
            }
        });

        const release = (event) => {
            const start = pointers.get(event.pointerId);
            pointers.delete(event.pointerId);
            if (film?.media) return;
            if (swallowRelease) {
                swallowRelease = pointers.size > 0;
                return;
            }
            if (event.type !== "pointerup" || !start || moved) return;

            const point = local(event);
            if (event.pointerType === "mouse") {
                if (event.button === 0) toggleAt(point, true);
                return;
            }
            const now = performance.now();
            if (now - lastTap.time < 320 && Math.hypot(point.x - lastTap.x, point.y - lastTap.y) < 30) {
                toggleAt(point, false);
                lastTap.time = 0;
            } else {
                lastTap = { time: now, x: point.x, y: point.y };
            }
        };
        stage.addEventListener("pointerup", release);
        stage.addEventListener("pointercancel", release);

        window.addEventListener("resize", () => {
            if (box.hidden || !image.naturalWidth) return;
            measure();
            keepInside();
            apply(false);
        });

        document.addEventListener("keydown", (event) => {
            if (box.hidden) return;
            if (event.key === "Escape") close();
            else if (event.key === "+" || event.key === "=") zoomInButton.click();
            else if (event.key === "-") zoomOutButton.click();
            else if (event.key === "0") fitButton.click();
        });
    }
})();
