// Namespace top level
// written by andrew dupont, optimized by addy osmani
function extend(destination, source) {
    var toString = Object.prototype.toString,
        objTest = toString.call({});
    for (var property in source) {
        if (source[property] && objTest == toString.call(source[property])) {
            destination[property] = destination[property] || {};
            extend(destination[property], source[property]);
        } else {
            destination[property] = source[property];
        }
    }
    return destination;
};

var Lkr = {};


extend(Lkr, {
    Plan: {
        Setting: {
            Map: {
                mapResizeTolerance: 0.25,    // skillnaden mellan nuvarande och ny storlek på kartbehållare (0 - 1)
                mapResizeTimespan: 500,      // i millisekunder
                backGroundLayerSettings: {
                    defaultMapByLayer: 'landskrona-bakgrundskarta-color-notext',
                    backgroundtypes: [
                        {
                            title: 'Landskrona',
                            credential: {
                                token: '1f50918ab6164fbf84a511679af61c9f'
                            },
                            maps: [
                                {
                                    title: 'Landskrona Bakgrundskarta färg textfri',
                                    url: 'https://map-services.landskrona.se/maps/v1/bakgrundskarta-color-notext',
                                    params: {
                                        layers: 'landskrona-bakgrundskarta-color-notext'
                                    }
                                },
                                {
                                    title: 'Landskrona Bakgrundskarta nedtonad textfri',
                                    url: 'https://map-services.landskrona.se/maps/v1/bakgrundskarta-toned-down-notext',
                                    params: {
                                        layers: 'landskrona-bakgrundskarta-toned-down-notext'
                                    }
                                }
                            ]
                        },
                        /*
                        // Externa resurser. Bygger på att reverse proxy fungerar. Per 2025-08-14 gör en implementation i Web.config inte det.
                        {
                            title: 'Lantmäteriet',
                            credential: {
                                basic: {
                                    user: 'land0005',
                                    password: 'OyP5gL4B8fhI'
                                }
                            },
                            maps: [
                                {
                                    title: 'Topografiska webbkartan färg',
                                    // url: 'https://maps.lantmateriet.se/topowebb/wms/v1?request=GetCapabilities&version=1.1.1',
                                    // url: 'https://maps.lantmateriet.se/topowebb/wms/v1',
                                    url: '/app/plan/proxy/lm-topowebb/topowebb/wms/v1/',
                                    params: {
                                        layers: 'topowebbkartan'
                                    }
                                }
                            ]
                        },
                        {
                            title: 'Trafikverket',
                            credential: {
                            },
                            maps: [
                                {
                                    title: 'Nätinformation',
                                    url: '/app/plan/proxy/trv-netinfo_1_8/MapService/wms.axd/NetInfo_1_8/',
                                    params: {
                                        layers: 'Vagtrafiknat'
                                    },
                                }
                            ]
                        }
                        */
                    ]
                },
                hopp: [
                    // {
                    //     name: 'Länk till Handläggarkartan (CSM test 1)',
                    //     active: true,
                    //     link: 'https://karta-intern.landskrona.se/spatialmap?ignorefavorite=true&profile=csm_standard_profile&selectorgroups=planer+planer_gallande_planer&layers=theme-bakgrundskarta_color_notext+theme-planytor_andrade_y+theme-planytor_y&opacities=1+1+1&mapext={e_min}+{n_min}+{e_max}+{n_max}'
                    // },
                    // {
                    //     name: 'Länk till Handläggarkartan (CSM test 2)',
                    //     active: true,
                    //     link: 'https://karta-intern.landskrona.se/spatialmap?ignorefavorite=true&profile=csm_standard_profile&wkt=POLYGON(({e_min}+{n_min}%2C{e_min}+{n_max}%2C{e_max}+{n_max}%2C{e_max}+{n_min}%2C{e_min}+{n_min}))&page=content-showwkt&selectorgroups=planer+planer_gallande_planer&layers=theme-bakgrundskarta_color_notext+theme-planytor_andrade_y+theme-planytor_y&opacities=1+1+1&mapext={e_min}+{n_min}+{e_max}+{n_max}'
                    // },
                    {
                        name: 'Länk till Handläggarkartan',
                        active: true,
                        link: 'https://karta-intern.landskrona.se/spatialmap?ignorefavorite=true&profile=handlaggarkartan&wkt={wkt}&page=content-showwkt&selectorgroups=planer+planer_gallande_planer&layers=theme-bakgrundskarta_color_notext+theme-planytor_andrade_y+theme-planytor_y&opacities=1+1+1&mapext={e_min}+{n_min}+{e_max}+{n_max}'
                    },
                    {
                        name: 'Handläggarkartan (gamla)',
                        active: true,
                        link: 'https://geodata-handlaggare.landskrona.local/mapserver2016/fusion/templates/mapguide/gsviewer_a/index.html?applicationdefinition=Library://LANDSKRONA/Webblayouter/Landskrona_handlaggare.ApplicationDefinition&extent={e_min}%2C{n_min}%2C{e_max}%2C{n_max}&theme=Library%3A%2F%2FLANDSKRONA%2FKartor%2FLandskrona_handlaggare.MapDefinition&showgroups=Gallande_planer'
                    },
                ]
            }
        },
        Dokument: {
            //resolvedClientUrl: '',
            isPlansSearched: false,
            types: null,
            initExpColAll: false,
            planListInfo: null,
            nbrOfPlanHits: 0,
            nbrOfPlanBoms: 0,
            currentWindowSizeWidth: null,
            currentWindowSizeHeight: null
        },
        AjaxCalls: {
            getPlansDocs: null,
            putMapOfPlan: null,
            Delay: 500                      // i millisekunder, saktar ner funktion med max motsvarande angiven tid
        }
    }
})