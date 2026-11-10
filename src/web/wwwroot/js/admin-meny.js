document.addEventListener("DOMContentLoaded", () => {
    const appShell = document.getElementById("appShell");
    const sidebarToggle = document.getElementById("sidebarToggle");
    const sidebarResizer = document.getElementById("sidebarResizer");
    const root = document.documentElement;

    const MIN_WIDTH = 200;
    const MAX_WIDTH = 480;
    const DEFAULT_WIDTH = 260;

    let sidebarWidth = DEFAULT_WIDTH;
    let previousExpandedWidth = DEFAULT_WIDTH;

    let isResizing = false;
    let resizePointerId = null;

    function setSidebarWidth(width) {
        sidebarWidth = Math.max(
            MIN_WIDTH,
            Math.min(MAX_WIDTH, width)
        );

        previousExpandedWidth = sidebarWidth;

        root.style.setProperty(
            "--sidebar-width",
            `${sidebarWidth}px`
        );

        sidebarResizer.setAttribute(
            "aria-valuenow",
            String(Math.round(sidebarWidth))
        );
    }

    sidebarToggle.addEventListener("click", () => {
        const collapsed =
            appShell.classList.toggle("sidebar-collapsed");

        sidebarToggle.setAttribute(
            "aria-expanded",
            String(!collapsed)
        );

        sidebarToggle.setAttribute(
            "aria-label",
            collapsed
                ? "Expandera menyn"
                : "Fäll ihop menyn"
        );

        sidebarToggle.title =
            collapsed
                ? "Expandera menyn"
                : "Fäll ihop menyn";

        if (!collapsed) {
            setSidebarWidth(previousExpandedWidth);
        }
    });

    sidebarResizer.addEventListener("pointerdown", event => {
        if (appShell.classList.contains("sidebar-collapsed")) {
            return;
        }

        if (event.button !== 0) {
            return;
        }

        isResizing = true;
        resizePointerId = event.pointerId;

        sidebarResizer.setPointerCapture(event.pointerId);

        document.body.classList.add("sidebar-resizing");

        event.preventDefault();
    });

    sidebarResizer.addEventListener("pointermove", event => {
        if (!isResizing || event.pointerId !== resizePointerId) {
            return;
        }

        setSidebarWidth(event.clientX);
    });

    function stopResizing(event) {
        if (!isResizing || event.pointerId !== resizePointerId) {
            return;
        }

        isResizing = false;
        resizePointerId = null;

        document.body.classList.remove("sidebar-resizing");
    }

    sidebarResizer.addEventListener("pointerup", stopResizing);
    sidebarResizer.addEventListener("pointercancel", stopResizing);

    sidebarResizer.addEventListener("keydown", event => {
        if (appShell.classList.contains("sidebar-collapsed")) {
            return;
        }

        let newWidth = sidebarWidth;

        if (event.key === "ArrowLeft") {
            newWidth -= 10;
        } else if (event.key === "ArrowRight") {
            newWidth += 10;
        } else if (event.key === "Home") {
            newWidth = MIN_WIDTH;
        } else if (event.key === "End") {
            newWidth = MAX_WIDTH;
        } else {
            return;
        }

        event.preventDefault();
        setSidebarWidth(newWidth);
    });

    document.querySelectorAll(".submenu-toggle").forEach(button => {
        button.addEventListener("click", () => {
            if (appShell.classList.contains("sidebar-collapsed")) {
                return;
            }

            const submenuId =
                button.getAttribute("aria-controls");

            const submenu =
                document.getElementById(submenuId);

            if (!submenu) {
                return;
            }

            const expanded =
                button.getAttribute("aria-expanded") === "true";

            button.setAttribute(
                "aria-expanded",
                String(!expanded)
            );

            submenu.hidden = expanded;
        });
    });

    function normalizePath(path) {
        if (path.length > 1) {
            path = path.replace(/\/+$/, "");
        }

        return path.toLowerCase();
    }

    const currentPath =
        normalizePath(window.location.pathname);

    const links = Array.from(
        document.querySelectorAll(
            ".sidebar-menu a[href]"
        )
    );

    const activeLink = links.find(link => {
        const url = new URL(
            link.href,
            window.location.origin
        );

        return url.origin === window.location.origin &&
            normalizePath(url.pathname) === currentPath;
    });

    if (activeLink) {
        activeLink.classList.add("active");
        activeLink.setAttribute("aria-current", "page");

        const submenu =
            activeLink.closest(".submenu");

        if (submenu) {
            submenu.hidden = false;

            const parentItem =
                submenu.closest(".has-submenu");

            const parentButton =
                parentItem?.querySelector(
                    ":scope > .submenu-toggle"
                );

            if (parentButton) {
                parentButton.setAttribute(
                    "aria-expanded",
                    "true"
                );
            }
        }
    }

    setSidebarWidth(DEFAULT_WIDTH);
});
