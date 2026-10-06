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
    if (box) {
        const image = box.querySelector("img");
        const caption = box.querySelector("[data-lightbox-caption]");
        const closeButton = box.querySelector("[data-lightbox-close]");
        const close = () => {
            box.hidden = true;
            document.body.classList.remove("lightbox-open");
            if (image) image.removeAttribute("src");
        };

        document.querySelectorAll("[data-lightbox]").forEach((link) => {
            link.addEventListener("click", (event) => {
                event.preventDefault();
                if (!image) return;
                image.src = link.getAttribute("href");
                image.alt = link.dataset.title || "Photograph";
                if (caption) caption.textContent = link.dataset.title || "";
                box.hidden = false;
                document.body.classList.add("lightbox-open");
                closeButton?.focus();
            });
        });

        box.addEventListener("click", (event) => {
            if (event.target === box || event.target.closest("[data-lightbox-close]"))
                close();
        });

        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && !box.hidden) close();
        });
    }
})();
