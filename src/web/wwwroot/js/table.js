var urlBasePath = Lkr.Plan.Dokument.resolvedClientUrl;

/**
 * Skapar en sorterbar tabell och placerar i inskickad html-behållare av inskickad
 * platt data-array.
 * @argument(DOM-object) container - DOM-objekt genom JavaScript
 * @example
 * @argument(Array of Object) data - 
 * @argument(Object) options - 
 * @argument(Arrays) options.columns - 
 * @argument(Arrays) options.initialSort - 
 * @example initialSort : { key: "age", direction: "desc" }
 * eller för flera
 * initialSort : [
 *  { key: "status", direction: "asc" },
 *  { key: "age", direction: "desc" }
 * ]
 * @argument(Arrays) options.extraColumns - 
 */
class CustomTable {
    constructor(container, data, options = {}) {
        this.container = container;
        this.data = data;
        this.columns = options.columns || this._extractColumns(data);
        this.sortState = []; // [{key, direction}]
        this.extraColumns = options.extraColumns || [];

        
        // 🔒 Hantera kolumn som ska vara låst i sorteringen
        this._lockedSortColumn = this.columns.find(c => c.lockedSort);

        if (this._lockedSortColumn) {
            this.sortState.push({
                key: this._lockedSortColumn.key,
                direction: this._lockedSortColumn.lockedSort
            });
        }

        // Initial sortering
        if (options.initialSort) {
            this._setInitialSort(options.initialSort);
        }

        // Spara originalsortering för senare reset
        this._originalSortState = JSON.parse(JSON.stringify(this.sortState));

        this._render();

        // Kör vid scroll och resize
        window.addEventListener("scroll", this.updateTableFunctionsPosition);
        window.addEventListener("resize", this.updateTableFunctionsPosition);
    }

    _setInitialSort(initialSort) {
    // Tillåt både objekt och lista av objekt
    if (!Array.isArray(initialSort)) {
        initialSort = [initialSort];
    }

    this.sortState = initialSort.map(s => ({
        key: s.key,
        direction: s.direction === "desc" ? "desc" : "asc"
    }));
}

    _extractColumns(data) {
        // Använd första objektet för att bestämma kolumnnamn
        if (!data || data.length === 0) return [];
        return Object.keys(data[0]).map(key => ({
            key: key,
            label: key
        }));
    }

    _render() {
        this.container.innerHTML = "";
        const table = document.createElement("table");
        table.classList.add("custom-table","table","table-striped","table-hover","table-sm","table-group-divider");

        const thead = this._createHeader();
        const tbody = this._createBody();

        table.appendChild(thead);
        table.appendChild(tbody);
        this.container.appendChild(table);

        // Knappar för olika tabellfunktioner
        const divFunctions = document.createElement("div");
        divFunctions.classList.add("table-functions");
        const resetButton = document.createElement("button");
        resetButton.classList.add("button-plan","button-plan-animation","button-reset-sorting");
        resetButton.addEventListener("click", () => this.resetSort());
        resetButton.title = "Initial sortering";
        const clearButton = document.createElement("button");
        clearButton.classList.add("button-plan","button-plan-animation","button-clear-sorting");
        clearButton.addEventListener("click", () => this.clearSort());
        clearButton.title = "Plocka bort sortering (enl. hämtad data)";
        const closeDocumentButton = document.createElement("button");
        closeDocumentButton.classList.add("button-plan", "button-plan-animation", "button-close-document-row");
        closeDocumentButton.addEventListener("click", () => this.closeDocumenRow());
        closeDocumentButton.title = "Stänger visning av plans plandokument";
        const toTopButton = document.createElement("button");
        toTopButton.classList.add("button-plan","button-plan-animation","button-to-top");
        toTopButton.addEventListener("click", () => this.toTop());
        toTopButton.title = "Till toppen";
        
        divFunctions.appendChild(resetButton);
        divFunctions.appendChild(clearButton);
        divFunctions.appendChild(closeDocumentButton);
        divFunctions.appendChild(toTopButton);
        this.container.appendChild(divFunctions);

        this.updateTableFunctionsPosition();

    }

    _createHeader() {
        const thead = document.createElement("thead");
        thead.classList.add("table-dark", "no-hover");
        const row = document.createElement("tr");

        // Data-kolumner
        this.columns.forEach(col => {
            const th = document.createElement("th");
            th.style.cursor = "pointer";

            if (col.type === "hidden") {
                th.style.display = "none"; // eller visibility: "hidden"
            }

            const labelSpan = document.createElement("span");
            labelSpan.textContent = col.label;

            const iconSpan = document.createElement("span");
            iconSpan.className = "sort-icon";
            iconSpan.style.marginLeft = "6px";

            // Visa ikon om kolumnen är i sortState
            const sortEntry = this.sortState.find(s => s.key === col.key);
            // if (sortEntry) {
            //     iconSpan.textContent = sortEntry.direction === "asc" ? "▲" : "▼";
            // }
            iconSpan.textContent = sortEntry
                ? (sortEntry.direction === "asc" ? "▲" : "▼")
                : "";

            th.appendChild(labelSpan);
            th.appendChild(iconSpan);

            th.addEventListener("click", (e) => this._handleSort(col.key, e.shiftKey));

            row.appendChild(th);
        });

        // Extra kolumner, har ingen sorteringsikon
        this.extraColumns.forEach(extra => {
            const th = document.createElement("th");
            th.textContent = extra.label || "";
            row.appendChild(th);
        });

        thead.appendChild(row);
        return thead;
    }

    _createBody() {
        const tbody = document.createElement("tbody");

        const sortedData = this._applySort([...this.data]);
        sortedData.forEach(item => {
            const row = document.createElement("tr");

            this.columns.forEach(col => {
                const td = document.createElement("td");

                if (col.type === "hidden") {
                    td.style.display = "none"; // alternativt visibility: "hidden"
                }

                if (typeof col.render === "function") {
                    const rendered = col.render(item[col.key], item);
                    if (rendered instanceof Node) {
                        td.appendChild(rendered);
                    } else {
                        td.textContent = rendered;
                    }
                } else {
                    td.textContent = item[col.key];
                }
                row.appendChild(td);
            });

            // Rendera extra kolumner (ex. knappar)
            this.extraColumns.forEach(extra => {
                const td = document.createElement("td");
                if (typeof extra.render === "function") {
                    td.appendChild(extra.render(item));
                }
                row.appendChild(td);
            });

            tbody.appendChild(row);
        });

        return tbody;
    }

    _handleSort(key, multi) {
        // Om kolumnen är låst i sortering
        if (this._lockedSortColumn && key === this._lockedSortColumn.key) {
            const locked = this.sortState[0];
            locked.direction = locked.direction === "asc" ? "desc" : "asc";
            this._render();
            return;
        }

        let existing = this.sortState.find(s => s.key === key);
        if (existing) {
            existing.direction = existing.direction === "asc" ? "desc" : "asc";
        } else {
            if (!multi) {
                // Rensa sortering men behåll låst kolumn om den finns
                if (this._lockedSortColumn) {
                    this.sortState = [this.sortState[0]];
                } else {
                    this.sortState = [];
                }
            }

            this.sortState.push({ key, direction: "asc" });
        }

        // Se till att låst kolumn ALLTID ligger först
        if (this._lockedSortColumn) {
            const lockedKey = this._lockedSortColumn.key;
            this.sortState.sort((a, b) => {
                if (a.key === lockedKey) return -1;
                if (b.key === lockedKey) return 1;
                return 0;
            });
        }

        this._render();
    }

    _applySort(data) {
        if (this.sortState.length === 0) return data;
        
        return data.sort((a, b) => {
            for (const { key, direction } of this.sortState) {
                const dir = direction === "asc" ? 1 : -1;
                if (a[key] < b[key]) return -1 * dir;
                if (a[key] > b[key]) return 1 * dir;
            }
            return 0;
        });
    }

    resetSort() {
        // Återställ sorteringsordningen
        this.sortState = JSON.parse(JSON.stringify(this._originalSortState));

        // Rendera tabellen igen
        this._render();
    }

    clearSort() {
        // Möjlighet till att återgå till ingen sortering alls, alltså originalordningen i datat.
        this.sortState = [];   // Ta bort all sortering inkl. låsta
        this._render();
    }

    closeDocumenRow() {
        const r = document.getElementById("DocumentRow");
        if (r) {
            const prevButton = r.previousSibling.cells[6].querySelectorAll("button")[0];
            prevButton.classList.add("button-plan-animation");
            prevButton.style.cursor = "pointer";
            prevButton.disabled = false;
            r.remove();
        }
    }

    toTop() {
        window.scrollTo({
            top: 0,
            left: 0,
            behavior: "smooth",
        });

        document.getElementById("searchJTablePlanList").focus();
    }


    updateTableFunctionsPosition() {
        let table = this.container.getElementsByTagName('table')[0];
        let divFunctions = table.nextElementSibling;

        let rect = table.getBoundingClientRect();

        // Om tabellens top fortfarande är synlig i viewporten
        if (rect.top >= 0) {
            divFunctions.style.position = "absolute";

            let wrapperRect = this.container.getBoundingClientRect();
            const offsetLeft = rect.right - wrapperRect.left + 10;

            divFunctions.style.left = offsetLeft + "px";
            divFunctions.style.top = "10px";
        }
        else {
            // Lås till viewporten
            divFunctions.style.position = "fixed";
            divFunctions.style.left = (rect.right + 10) + "px";
            divFunctions.style.top = "10px";
        }
    }

}
