// Initial kartutbredning
const initialMapExtent = [96000, 6184000, 123000, 6205000];
// EPSG:3008
// Tas från https://epsg.io/3008.proj4
const epsgProjectionDefintion = '+proj=tmerc +lat_0=0 +lon_0=13.5 +k=1 +x_0=150000 +y_0=0 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs +type=crs';
var urlBasePath = Lkr.Plan.Dokument.resolvedClientUrl;
const backGroundLayerSettings = Lkr.Plan.Setting.Map.backGroundLayerSettings;
const jumps = Lkr.Plan.Setting.Map.hopp;



// Projektion och extension
if (checkProj4Exists()) {
    proj4.defs("EPSG:3008", epsgProjectionDefintion);
}
if (checkOpenLayerExists()) {
    ol.proj.proj4.register(proj4);
}
const projection = ol.proj.get("EPSG:3008");
projection.setExtent(initialMapExtent);





class MapBackground {

    constructor(placeholderId) {
        this.PlaceholderId = placeholderId;
        this.MapBackgroundStorageKey = 'plandokument-background';
        this.BackgroundLayers = this.MapBackgrounds;

        //rensar ev. loading gif i platshållaren
        let placeHolderDOM = this.PlaceHolderAsDOM;
        let content = placeHolderDOM.querySelector("div");
        if (content) {
            placeHolderDOM.removeChild(content);
        }


        // Skapar plats för kartan (höjd i DOM för kartcontainern) initialt
        this._updateMapHeightFromParentElmnt();

        // Event-lyssnare för storleksförändringar hos elementföräldern
        // M.h.a. webb standard API ResizeObserver, som verkar ha bra täckning hos webbläsare sedan 2020, https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserver#browser_compatibility
        const parentEl = placeHolderDOM.parentElement;
        if (parentEl) {
            this._resizeObserver = new ResizeObserver(() => {
                this._updateMapHeightFromParentElmnt();
                if (this.map) {
                    this.map.updateSize(); // Säkerställer att OL ritar om kartan
                }
            });
            this._resizeObserver.observe(parentEl);
        }



        // karta skapas och syns på webbsida
        this.map = new ol.Map({
            target: placeholderId,
            controls: [],
            view: new ol.View({
                projection: projection,
                center: [111111, 6196797],
                minZoom: 2,
                maxZoom: 11,
            })
        });

        this.map.addLayer(this.BackgroundLayers);


        const polygon = new ol.geom.Polygon([[[107727,6194253],[107765,6194170],[107847,6194200],[107807,6194283],[107727,6194253]]]);
        const vectorLayer = new ol.layer.Vector({
            source: new ol.source.Vector({
                features: [new ol.Feature({
                    geometry: polygon,
                    name: 'Stadshuset'
                })]
            }),
            style: this.OlDefaultSelectionVectorStyle
        });

        // this.map.addLayer(vectorLayer);
        // Initiera och addera OL-kontroll droplista för val av bakgrundskartor
        this.map.addControl(new OlMapBackgroundDropdownControl(this));

        // Definiera, initiera och addera OL-kontroll för att expandera kartan till dess initiala extension
        const htmlElementExtent = document.createElement("span");
        htmlElementExtent.className = "bi bi-arrows-fullscreen hand";
        const controlToExtent = new ol.control.ZoomToExtent({
            label: htmlElementExtent,
            tipLabel: 'Expandera till kartans initiala utbredning',
            className: 'ol-zoom-extent-map'
        });

        this.map.addControl(controlToExtent);

        // Definiera, initiera och addera OL-kontroll för att expandera kartan till dess initiala extension
        const htmlElementFullScreen = document.createElement("span");
        htmlElementFullScreen.className = "bi bi-box-arrow-up-left hand";
        const controlToFullScreen = new ol.control.FullScreen({
            label: htmlElementFullScreen,
            tipLabel: 'Helskärm',
            className: 'ol-fullscreen'
        });

        this.map.addControl(controlToFullScreen);

        // setTimeout(() => { 
        //     console.log('Väntar...'); 
        // }, 2000);

        this.map.getView().fit(initialMapExtent, { size: this.map.getSize() });
        // Alternativa initial utbredning genom att extension görs om till polygon
        // this.map.getView().fit(
        //         ol.geom.Polygon.fromExtent(initialMapExtent),
        //         { size: this.map.getSize() }
        //     );
        // this.map.getView().fit(polygon, { size: this.map.getSize() });

    };

    get PlaceHolderAsDOM() {
        return document.getElementById(this.PlaceholderId) || (console.error(`Element med ID '${this.PlaceholderId}' hittades inte`), null);
    }

    get MapBackgrounds() {
        const baseLayers = [];
        const storedValue = localStorage.getItem(this.MapBackgroundStorageKey);
        // Hämta alla tillåtna layers från backgroundtypes
        const allowedLayers = backGroundLayerSettings.backgroundtypes.flatMap(bgType =>
            bgType.maps.map(map => map.params.layers)
        );
        // Om storedValue är tomt eller inte finns i allowedLayers → använd default, annars storedValue
        let defaultValue = (!storedValue || !allowedLayers.includes(storedValue))
            ? backGroundLayerSettings.defaultMapByLayer
            : storedValue;

        // För alla bakgrundslager, bygg array med OpenLayers-lager
        backGroundLayerSettings.backgroundtypes.forEach(bg => {
            bg.maps.forEach(mapOpt => {
                // const layer = new ol.layer.Image({
                //     // extent: extent,
                //     source: new ol.source.ImageWMS({
                //         url: `${mapOpt.url}${bg.credential.token ? ('?token=' + bg.credential.token) : ''}`,
                //         params: { 'LAYERS': mapOpt.layer },
                //         ratio: 1
                //     }),
                //     visible: (mapOpt.layer === defaultValue),
                //     title: mapOpt.title
                // });
                // layer.getSource().on('imageloadstart', () => console.log('Bildladdning påbörjad'));
                // layer.getSource().on('imageloadend', () => console.log('Bildladdning klar'));
                // layer.getSource().on('imageloaderror', () => console.warn('Bildladdning misslyckades'));

                baseLayers.push(
                    // layer
                    new ol.layer.Image({
                        source: new ol.source.ImageWMS({
                            url: `${bg.credential.token ? (mapOpt.url + '?token=' + bg.credential.token) :
                                        (bg.credential.basic ? mapOpt.url : mapOpt.url)
                                    }`,
                            // Försökt till att autentisera LM:s visningstjänster genom https://username:password@maps.lantmateriet.se...
                            // Fungerar dock inte pga av att webbläsare inte tillåter detta. Behövs en proxy, https://chatgpt.com/share/685c6497-e0e0-8001-b48d-f161c09ac41d (rubrik: "WMS autentisering OpenLayers")
                            params: {
                                LAYERS: mapOpt.params.layers,
                                VERSION: '1.1.1',
                                FORMAT: 'image/png',
                                SRS: 'EPSG:3008'
                            },
                            ratio: 1,
                            /* 
                            // För felsökning av adresser och response content
                            imageLoadFunction: (image, src) => {
                                console.log("Laddar bild från URL:", src);
                                fetch(src, {mode: 'cors'})
                                    .then(r => r.blob())
                                    .then(b => {
                                        console.log("MIME-type:", b.type);
                                        return b.text();
                                    })
                                    .then(t => console.log("Första 200 tecken av responsen:", t.substring(0, 200)))
                                    .catch(err => console.error("Fel vid hämtning:", err));
                                image.getImage().src = src;
                            }
                             */
                        }),
                        visible: (mapOpt.params.layers === defaultValue),
                        title: mapOpt.title
                    })
                )
            });
        });

        return new ol.layer.Group({
            layers: baseLayers,
            visible: true
        });
    };

    get OlDefaultSelectionVectorStyle() {
        return [
/*             new ol.style.Style({
                stroke: new ol.style.Stroke({
                    // color: 'rgba(0,0,204,1)',
                    color: 'rgba(0,127,255,1)',
                    width: 4,
                })
            }), */
            new ol.style.Style({
                stroke: new ol.style.Stroke({
                    // color: 'rgba(0,0,204,0.5)',
                    // color: 'rgba(204, 48, 0, 1)',
                    color: 'rgba(0,127,255,1)',
                    width: 3,
                }),
                fill: new ol.style.Fill({
                    // color: 'rgba(0, 0, 255, 0.3)',
                    color: 'rgba(0,127,255,0.4)',
                }),
            }),
            new ol.style.Style({
                image: new ol.style.Circle({
                    radius: 5,
                    fill: new ol.style.Fill({
                        color: 'rgba(0,0,204,0.5)',
                    }),
                    stroke: new ol.style.Stroke({
                        color: 'rgba(0,0,204,0.7)',
                        width: 3,
                    })
                }),
            }),
        ];
    }

    get OlPlanVectorStyle() {
        return [
/*             new ol.style.Style({
                stroke: new ol.style.Stroke({
                    // color: 'rgba(0,0,204,1)',
                    color: 'rgba(0,127,255,1)',
                    width: 4,
                })
            }), */
            new ol.style.Style({
                stroke: new ol.style.Stroke({
                    // color: 'rgba(0,0,204,0.5)',
                    // color: 'rgba(204, 48, 0, 1)',
                    color: 'rgba(178, 56, 179,0.6)',
                    width: 2,
                }),
                fill: new ol.style.Fill({
                    // color: 'rgba(0, 0, 255, 0.3)',
                    color: 'rgba(178, 56, 179,0.3)',
                }),
                text: new ol.style.Text({
                    font: '10px arial',
                    overflow: true,
                    fill: new ol.style.Fill({ color: 'rgba(178, 56, 179, 1)' }),
                    stroke: new ol.style.Stroke({ color: 'rgba(255, 255, 255, 1)', width: 2 })
                })
            }),
            new ol.style.Style({
                image: new ol.style.Circle({
                    radius: 5,
                    fill: new ol.style.Fill({
                        color: 'rgba(178, 56, 179,0.5)',
                    }),
                    stroke: new ol.style.Stroke({
                        color: 'rgba(178, 56, 179,0.7)',
                        width: 3,
                    })
                }),
            }),

        ];
    }

    styleAllPlans(feature) {
        const style = this.OlPlanVectorStyle[0].clone();
        style.getText().setText(feature.getId()); // <-- Hämtar "id" från GeoJSON
        return style;
    }

    switchBackgroundLayer(layer) {

        // let isOptionTextNotLayer = false;

        this.BackgroundLayers.getLayers().forEach(bgLayer => {

            let LAYER;
            if (bgLayer.getSource() instanceof ol.source.ImageWMS) {
                LAYER = bgLayer.getSource().getParams().LAYERS;
            }

            bgLayer.setVisible(LAYER === layer);

            if (LAYER === layer) {
                localStorage.setItem(this.MapBackgroundStorageKey, layer);
                // isOptionTextNotLayer = true;
            }

        });

        // if (!isOptionTextNotLayer) {
        //   localStorage.setItem(this.MapBackgroundStorageKey, layer);
        // }

    };

    createLayerLegend() {
        // Skapa upp OL-kontroller för lagerlegend
        // Wrappa select + knapp i en container
        const wrapper = document.createElement("div");
        wrapper.classList.add('ol-layer-legend', 'ol-unselectable', 'ol-control');

        // Skapa ikonknapp
        const toggleBtn = document.createElement("button");
        toggleBtn.type = "button";
        toggleBtn.title = "Lager";
        toggleBtn.innerHTML = '<span class="bi bi-stack hand"></span>';
        // Skapa diven som ska öppnas
        const legendPanel = document.createElement("div");
        legendPanel.id = "olControlLayerPanel-" + this.PlaceholderId;
        legendPanel.classList.add("legend-panel");
        legendPanel.innerHTML = "<p>Lager</p>";

        toggleBtn.addEventListener("click", () => {
            legendPanel.classList.toggle("open");
            // Hämta span-taggen i knappen
            const iconSpan = toggleBtn.querySelector("span");

            // Lägg till/ta bort klass beroende på panelens status
            if (legendPanel.classList.contains("open")) {
                iconSpan.classList.add("invert-icon-black");
            } else {
                iconSpan.classList.remove("active-icon");
            }
        });

        wrapper.appendChild(legendPanel);
        wrapper.appendChild(toggleBtn);

        return wrapper;
    }

    createMapBackgroundDropdown(onChange) {
        const container = this.PlaceHolderAsDOM;
        if (!container) {
            return;
        }

        // Wrappa select + knapp i en container
        const wrapper = document.createElement("div");
        wrapper.classList.add("map-background-wrapper", 'ol-unselectable', 'ol-control');

        // Skapa <select>
        const select = document.createElement("select");
        select.name = 'Bakgrundskartor';
        select.id = this.PlaceholderId + '-backgrounds';
        select.classList.add('map-background-dropdown', 'form-select', 'me-1', 'd-none');

        const storedValue = localStorage.getItem(this.MapBackgroundStorageKey);
        const allowedLayers = backGroundLayerSettings.backgroundtypes.flatMap(bgType =>
            bgType.maps.map(map => map.params.layers)
        );

        let defaultValue = (!storedValue || !allowedLayers.includes(storedValue))
            ? backGroundLayerSettings.defaultMapByLayer
            : storedValue;

        backGroundLayerSettings.backgroundtypes.forEach(bg => {
            if (backGroundLayerSettings.backgroundtypes.length > 1) {
                const optGroup = document.createElement("optgroup");
                optGroup.label = bg.title;
                bg.maps.forEach(mapOpt => {
                    const option = this._createMapBackgroundOptions(mapOpt, defaultValue);
                    optGroup.appendChild(option);
                });
                select.appendChild(optGroup);
            } else {
                bg.maps.forEach(mapOpt => {
                    const option = this._createMapBackgroundOptions(mapOpt, defaultValue);
                    select.appendChild(option);
                });
            }
        });

        select.addEventListener("change", () => {
            const value = select.value;
            localStorage.setItem(this.MapBackgroundStorageKey, value);
            if (typeof onChange === "function") {
                onChange(value);
            }
            this.switchBackgroundLayer(value);
            this._checkBackgroundSelectOverflow(this.PlaceHolderAsDOM, select, toggleBtn);
        });

        // Skapa ikonknapp
        const toggleBtn = document.createElement("button");
        toggleBtn.type = "button";
        toggleBtn.title = "Växla bakgrundskarta";
        // toggleBtn.className = "btn btn-secondary d-none";
        toggleBtn.innerHTML = '<span class="bi bi-globe2 hand"></span>';

        toggleBtn.addEventListener("click", () => {
            if (!select.classList.contains("d-none")) {
                select.classList.add("d-none");
                toggleBtn.querySelector('span').classList.remove("invert-icon-black");
                select.focus();
            } else {
                select.classList.remove("d-none");
                toggleBtn.querySelector('span').classList.add("invert-icon-black");
            }
        });

        wrapper.appendChild(select);
        wrapper.appendChild(toggleBtn);

        // Kör initial overflow-koll
        window.addEventListener("resize", () => this._checkBackgroundSelectOverflow(this.PlaceHolderAsDOM, select, toggleBtn));
        window.addEventListener("load", () => this._checkBackgroundSelectOverflow(this.PlaceHolderAsDOM, select, toggleBtn));

        this._checkBackgroundSelectOverflow(this.PlaceHolderAsDOM, select, toggleBtn);

        return wrapper;
    }

    // kan anropas, om kartan tas bort, för att undvika minnesläckor av event-lyssnaren
    disconnectResizeObserver() {
        if (this._resizeObserver) {
            this._resizeObserver.disconnect();
        }
    }

    _createMapBackgroundOptions(mapOpt, defaultValue) {
        const option = document.createElement("option");
        option.value = mapOpt.params.layers;
        option.textContent = mapOpt.title;
        if (mapOpt.params.layers === defaultValue) {
            option.selected = true;
        }
        return option
    };

    _checkBackgroundSelectOverflow(container, select, button) {
        // Om dropdown visas
        if (!select.classList.contains('d-none')) {
            // 1. Hitta längsta option-text
            let longestText = "";
            for (let option of select.options) {
                if (option.text.length > longestText.length) {
                    longestText = option.text;
                }
            }

            const selectStyle = window.getComputedStyle(select);
            // 2. Skapa temporärt span för att mäta textbredd
            const tempSpan = document.createElement("span");
            tempSpan.style.visibility = "hidden";
            tempSpan.style.position = "absolute";
            tempSpan.style.whiteSpace = "nowrap";
            tempSpan.style.font = selectStyle.font; // samma font som select
            tempSpan.textContent = longestText;
            document.body.appendChild(tempSpan);
            let textWidth = tempSpan.offsetWidth || 0;
            tempSpan.remove();

            // 3. Lägg till padding och border från select
            const paddingLeft = parseFloat(selectStyle.paddingLeft) || 0;
            const paddingRight = parseFloat(selectStyle.paddingRight) || 0;
            const borderLeft = parseFloat(selectStyle.borderLeftWidth) || 0;
            const borderRight = parseFloat(selectStyle.borderRightWidth) || 0;

            // 4. Lägg till utrymme för pil/dropdown (kan variera mellan browser, här 20px som standard)
            const dropdownArrowWidth = 20;
            const extraWidth = parseFloat(getComputedStyle(document.documentElement).fontSize) * 2; // 2em i px

            const selectTotalWidth = textWidth + paddingLeft + paddingRight + borderLeft + borderRight + dropdownArrowWidth + extraWidth;
            let parentWidth = container.offsetWidth;

            // if (selectWidth > parentWidth || selectWidth === 0) {
            if (selectTotalWidth > parentWidth) {
                select.classList.add("d-none");
                button.querySelector('span').classList.remove("invert-icon-black");
            } else {
                select.classList.remove("d-none");
                button.querySelector('span').classList.add("invert-icon-black");
            }
        }
    }

    _updateMapHeightFromParentElmnt() {
        const placeHolderDOM = this.PlaceHolderAsDOM;
        if (!placeHolderDOM || !placeHolderDOM.parentElement) return;

        // let mapHeight = placeHolderDOM.getAttribute("dimension").split(",")[1];
        const parentHeight = placeHolderDOM.parentElement.offsetHeight;
        // ?.offsetHeight || 0; säkerställer att det inte kraschar om span saknas
        // const spanHeight = placeHolderDOM.querySelector("span")?.offsetHeight || 0;

        // Summera alla andra barns höjd än placeHolderDOM, ej pixelperfekt eftersom ev. marginaler utelämnas
        let otherElementsHeight = 0;
        Array.from(placeHolderDOM.children).forEach(child => {
            if (!child.classList.contains('ol-viewport')) {
                otherElementsHeight += child.offsetHeight;
            }
        });
        

        // mapHeight = mapHeight - spanHeight;

        // Kartans höjd = förälderns höjd minus ev. <span>-höjd
        // placeHolderDOM.style.height = mapHeight + 'px';
        // placeHolderDOM.style.height = (parentHeight - spanHeight) + 'px';
        // Kartans höjd = förälderns höjd minus ev. andra elements höjder
        placeHolderDOM.style.height = (parentHeight - otherElementsHeight) + 'px';
    }

    async addPlan(planIDsAsStringArray) {
        const vectorLayer = await this._loadPlan(planIDsAsStringArray);
        if (vectorLayer) {
            this.map.addControl(new OlMapLayerLegendControl(this));

            // Skapa legendobjekt
            const legendLabel = document.createElement("label");
            legendLabel.classList.add('hand');
            const layerToggler = document.createElement("input");
            layerToggler.type = "checkbox";
            layerToggler.checked = true;
            layerToggler.addEventListener("click", () => {
                const visible = layerToggler.checked;
                vectorLayer.setVisible(visible);
            });
            legendLabel.appendChild(layerToggler);
            legendLabel.append("Sökt plan");

            const legend = document.getElementById("olControlLayerPanel-" + this.PlaceholderId);
            legend.appendChild(legendLabel);

            this.addAllPlans();
            this.map.addLayer(vectorLayer);

            const source = vectorLayer.getSource();
            const features = source.getFeatures();

            // Skapa upp OL-kontroller för att zooma till planens utbredning
            const htmlElementObject = document.createElement("span");
            htmlElementObject.className = "bi bi-fullscreen-exit hand";
            htmlElementObject.setAttribute("id", "olControlSetObjectExtentPlan");
            let extentAsGeometry = ol.geom.Polygon.fromExtent(
                source.getExtent()
            );
            extentAsGeometry.scale(1.25);
            const controlToObjects = new ol.control.ZoomToExtent({
                label: htmlElementObject,
                tipLabel: 'Expandera till planen',
                extent: extentAsGeometry.getExtent(),
                className: 'ol-zoom-extent-object'
            });
            
            this.map.addControl(controlToObjects);

            // Initial zoom av kartvyn till planen
            if (features.length > 0) {
                const extent = source.getExtent();

                this.map.getView().fit(extent, {
                    size: this.map.getSize(),
                    padding: [20, 20, 20, 20], // valfritt: lämnar lite luft runt objekten
                    duration: 500 // mjuk animering
                });
            }

        }
    }

    tryParseJson(str) {
        try {
            return JSON.parse(str);
        } catch {
            return null; // eller t.ex. {} om du vill ha tomt objekt
        }
    }

    async _loadPlan(planIDsAsStringArray) {
        try {
            const response = await fetch(urlBasePath + '/api/PlanGeometryAsGeoJSON/filterbyjson', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=UTF-8'
                },
                body: JSON.stringify(planIDsAsStringArray)
            });

            if (!response.ok) throw new Error("Misslyckades att hämta geometri");

            const msg = await response.json();
            
            // msg.geoJSON är en sträng som i sig är ett JSON-objekt (GeoJSON)
            const geojsonData = this.tryParseJson(msg.geoJSON);
            if (Object.keys(geojsonData).length === 0) {
                console.warn(`Ingen geometri som GeoJSON för plan med id ${planIDsAsStringArray}`);
                return null;
            }
            const vectorSource = new ol.source.Vector({
                features: new ol.format.GeoJSON().readFeatures(geojsonData, {
                    dataProjection: 'EPSG:3008',   // anpassa till datakällan
                    featureProjection: 'EPSG:3008' // anpassa till kartan
                })
            });

            return new ol.layer.Vector({
                source: vectorSource,
                style: this.OlDefaultSelectionVectorStyle
            });

        } catch (err) {
            console.error(`Fel vid hämtning av geometri för plan med id ${planIDsAsStringArray}: `, err);
            return null;
        }
    }

    /**
     * Laddar alla planer
     * @param {boolean} visibleStart - anger om lagret ska vara synligt från start.
     *                                  Endast true/false accepteras. Allt annat => false.
     */
    async addAllPlans(layerVisible = false) {
        const vectorLayer = await this._loadAllPlans(layerVisible);
        if (vectorLayer) {
            //TODO: Kontrollera om lagerlegend finns laddad

            // Skapa legendobjekt
            const legendLabel = document.createElement("label");
            legendLabel.classList.add('hand');
            const layerToggler = document.createElement("input");
            layerToggler.type = "checkbox";
            layerToggler.checked = layerVisible;
            layerToggler.addEventListener("click", () => {
                const visible = layerToggler.checked;
                vectorLayer.setVisible(visible);
            });
            legendLabel.appendChild(layerToggler);
            legendLabel.append("Alla övriga planer");

            // Kontrollera om legend-elementet redan finns i DOM, avsedd för om endast en karta på sidan (ingen sökning)
            let legend = document.getElementById("olControlLayerPanel-" + this.PlaceholderId);

            if (!legend) {
                // Finns ingen legend → skapa kontroll och lägg till den i kartan
                this.map.addControl(new OlMapLayerLegendControl(this));

                legend = document.getElementById("olControlLayerPanel-" + this.PlaceholderId);
            }

            legend.appendChild(legendLabel);

            this.map.addLayer(vectorLayer);

            // Popup overlay från ol-ext
            const popup = new ol.Overlay.Popup({
                popupClass: "default hand", // css-klass
                closeBox: true,
                positioning: 'auto',
                // onshow: () => console.log("Popup öppnad"),
                // onclose: () => console.log("Popup stängd"),
                autoPan: {
                    animation: { duration: 250 }
                }
            });
            this.map.addOverlay(popup);

            // Klick-händelse för att visa popup
            this.map.on("singleclick", (evt) => {
                // Kolla om man klickar på en feature i just detta lager
                this.map.forEachFeatureAtPixel(evt.pixel, (feature, layer) => {
                    if (layer === vectorLayer) {
                        const id = feature.getId();
                        const props = feature.getProperties();
                        let featureExtension = feature.getGeometry().getExtent();

                        let scaledGeometryExtension = ol.geom.Polygon.fromExtent(
                            feature.getGeometry().getExtent()
                        );
                        scaledGeometryExtension.scale(1 + Lkr.Plan.Setting.Map.mapResizeTolerance);
                        const scaledFeatureExtension = scaledGeometryExtension.getExtent();
                        const wktFormat = new ol.format.WKT()
                        const featureAsWkt = wktFormat.writeFeature(feature);

                        let jumpLinks = "";
                        jumps.forEach(item => {
                            if (item.active) {
                                if (item.link.includes("{wkt}")) {
                                    jumpLinks += `<p><a href="${item.link.replace("{wkt}", featureAsWkt).replaceAll("{e_min}", scaledFeatureExtension[0]).replaceAll("{n_min}", scaledFeatureExtension[1]).replaceAll("{e_max}", scaledFeatureExtension[2]).replaceAll("{n_max}", scaledFeatureExtension[3])}" target="_blank">${item.name}</a><span class="linkNewWindow" style="top: 0px; left: 0px;" title="Öppnar länk i nytt webbläsarfönster"></span></p>`
                                }
                                else {
                                    jumpLinks += `<p><a href="${item.link.replaceAll("{e_min}", featureExtension[0]).replaceAll("{n_min}", featureExtension[1]).replaceAll("{e_max}", featureExtension[2]).replaceAll("{n_max}", featureExtension[3])}" target="_blank">${item.name}</a><span class="linkNewWindow" style="top: 0px; left: 0px;" title="Öppnar länk i nytt webbläsarfönster"></span></p>`
                                }
                            }
                        });
                        // Bygg popup-innehåll
                        const html = `
                        <div style="min-width:200px; font-size: 1.2em;">
                            <h6>Plan ${id}</h6>
                            <p><a href="${Lkr.Plan.Dokument.resolvedClientUrl}/dokument/${id}" target="_blank">Dokument<span class="linkNewWindow" style="top: 0px; left: 0px;" title="Öppnar länk i nytt webbläsarfönster"></span></a></p>
                            ${jumpLinks}
                        </div>`;
                        popup.show(evt.coordinate, html);
                    }
                });
            });

            // 🔑 Ändra muspekare när man hovrar över feature i detta lager
            this.map.on("pointermove", (evt) => {
                const hit = this.map.hasFeatureAtPixel(evt.pixel, {
                    layerFilter: (layer) => layer === vectorLayer
                });

                this.map.getTargetElement().style.cursor = hit ? "pointer" : "";
            });
        }
    }

    async _loadAllPlans(layerVisible) {
        const cacheKey = "plansGeoJsonCache";

        try {
            // Kolla om cache finns
            const cached = localStorage.getItem(cacheKey);
            if (cached) {
                const cachedObj = JSON.parse(cached);

                // Kolla om cachen är från samma dag        
                const today = new Date().toISOString().split("T")[0]; // YYYY-MM-DD
                if (cachedObj.date === today) {
                    // Filtrera bort sökt plan från övriga alla andra planer om akt-elementet hittas (med andra ord sökt plan)
                    // const geojsonDataFiltered = cachedObj.features.features.filter(item => {
                    //     return item.id !== document.getElementById("akt-" + this.PlaceholderId.split("-")[1]).innerText;
                    // });
                    const aktId = document.getElementById("akt-" + this.PlaceholderId.split("-")[1])?.innerText;
                    const geojsonDataFiltered = cachedObj.features.features.filter(item =>
                        aktId ? item.id !== aktId : true
                    );
                    // Skapa giltig GeoJSON
                    let featuresFiltered = {};
                    if (geojsonDataFiltered.length === 1) {
                        featuresFiltered = geojsonDataFiltered[0];
                    }
                    else if (geojsonDataFiltered.length > 1) {
                        featuresFiltered = {
                            type: "FeatureCollection",
                            features: geojsonDataFiltered
                        };
                    }

                    const vectorSource = new ol.source.Vector({
                        features: new ol.format.GeoJSON().readFeatures(featuresFiltered, {
                            dataProjection: 'EPSG:3008',   // anpassa till datakällan
                            featureProjection: 'EPSG:3008' // anpassa till kartan
                        })
                    });

                    return new ol.layer.Vector({
                        source: vectorSource,
                        // style: this.OlPlanVectorStyle,
                        // style: this.styleAllPlans, // Skapar ol-mapbackground.js:347 Uncaught TypeError: Cannot read properties of undefined (reading 'OlPlanVectorStyle')
                        style: (feature) => this.styleAllPlans(feature), // Arrrow-funktion för att komma åt rätt this, som annars bryts av OL
                        visible: layerVisible
                    });
                }
            }

            const response = await fetch(urlBasePath + '/api/PlanGeometryAsGeoJSON/all');
            if (!response.ok) throw new Error("Misslyckades att hämta planers alla geometrier som GeoJSON");

            // ASP.NET .asmx returnerar { d: "<JSON-string>" }
            const geojsonData = await response.json();

            if (!geojsonData || geojsonData.length === 0) {
                console.warn(`Inga plangeometrier som GeoJSON`);
                return null;
            }

            // msg.d är en sträng som i sig är ett JSON-objekt (GeoJSON)
            // Filtrerar bort sökt planer från alla planer och anpassar GeoJSON-data till enskild feature eller vid fler till FeatureCollection
            let features = {};
            if (geojsonData.length === 1) {
                features = geojsonData[0].result;
            }
            else if (geojsonData.length > 1) {
                features = {
                    type: "FeatureCollection",
                    features: geojsonData.map(f => JSON.parse(f.geoJSON))
                };
            }

            // Filtrera bort sökt plan från övriga alla andra planer
            const aktId = document.getElementById("akt-" + this.PlaceholderId.split("-")[1])?.innerText;
            const geojsonDataFiltered = geojsonData.filter(item => {
                // const feature = JSON.parse(item.result);
                const feature = item.geoJSON;
                return aktId ? feature.id !== aktId : true
            });
            // Skapa giltig GeoJSON
            let featuresFiltered = {};
            if (geojsonDataFiltered.length === 1) {
                featuresFiltered = geojsonDataFiltered[0].geoJSON;
            }
            else if (geojsonDataFiltered.length > 1) {
                featuresFiltered = {
                    type: "FeatureCollection",
                    features: geojsonDataFiltered.map(f => JSON.parse(f.geoJSON))
                };
            }

            // Cache:a data med dagens datum
            localStorage.setItem(cacheKey, JSON.stringify({
                date: new Date().toISOString().split("T")[0],
                features
            }));

            // console.log("JS featureCollectionGeoJsonData", features);
            // console.log("JSON featureCollectionGeoJsonData stringify", JSON.stringify(features));

            const vectorSource = new ol.source.Vector({
                features: new ol.format.GeoJSON().readFeatures(featuresFiltered, {
                    dataProjection: 'EPSG:3008',   // anpassa till datakällan
                    featureProjection: 'EPSG:3008' // anpassa till kartan
                })
            });

            return new ol.layer.Vector({
                source: vectorSource,
                // style: this.OlPlanVectorStyle,
                // style: this.styleAllPlans,
                style: (feature) => this.styleAllPlans(feature),
                visible: layerVisible
            });

        } catch (err) {
            console.error(`Fel vid hämtning av planers geometri som GeoJSON: `, err);
            return null;
        }
    }

    
};

// Innre dropdownkontrollklass som får instansen
class OlMapBackgroundDropdownControl extends ol.control.Control {
    constructor(mapInstance) {
        const element = mapInstance.createMapBackgroundDropdown(layer => {
            mapInstance.switchBackgroundLayer(layer);
        });

        super({
            element: element,
        });
    }
}

// Innre lagerlegendklass som får instansen
class OlMapLayerLegendControl extends ol.control.Control {
    constructor(mapInstance) {

        super({
            element: mapInstance.createLayerLegend(),
        });

    }
}



function checkOlExtExists() {
    if (ol.ext) {
        return true;
    }
    else {
        console.warn("ol-ext saknas");
        return false;
    }
}

function checkOpenLayerExists() {
    if (ol) {
        return true;
    }
    else {
        console.warn("OpenLayers saknas");
        return false;
    }
}

function checkProj4Exists() {
    if (proj4) {
        return true;
    }
    else {
        console.warn("Proj4 saknas");
        return false;
    }
}