/**
 * Med en array av plandokumentsinformation enl. nedan, skapas div-element med 
 * verktyg för nedladdning av dokument och en formaterad punktlistning av 
 * dokumenten med individuella hyperlänkar.
 * plansDocs = [
 *    {
 *       planId,
 *       planNamn,
 *       documents: [
 *         {
 *           PLAN_ID,
 *           NAME,
 *           EXTENTION,
 *           SIZE,
 *           PATH,
 *           PATHVIRTUAL,
 *           DOCUMENTTYPE,
 *           FINDTYPE,
 *           DOCUMENTPART,
 *           THUMNAILPATH,
 *           THUMNAILINDICATION
 *         }
 *       ]
 *     }
 *   ]
 * @return {HTML} Avsnittsindelad punktlista med länkade dokument
 */
async function buildPlanDocuments(plansDocs) {
  if (!Array.isArray(plansDocs) || plansDocs.length === 0) {
    throw new Error("plansDocs saknas eller är tom.");
  }

  const documentTypes = await Lkr.Plan.Dokument.types;
  if (!documentTypes) {
    throw new Error("Dokumenttyper saknas.");
  }


  /* =========================
     Root containers
     ========================= */
  const root = document.createElement("div");
  // root.className = "plan-documents";
  root.classList.add("plan-documents");
  root.classList.add("planContent-left");

  const toolbar = document.createElement("div");
  toolbar.className = "planContent-tools";
  toolbar.style.marginBottom = "1em";
  toolbar.style.position = "relative";

  const content = document.createElement("div");
  content.className = "plan-documents-content";
  content.style.paddingLeft = "2.2em"

  root.append(toolbar, content);

  /* =========================
     Init document type state
     ========================= */
  documentTypes.forEach(dt => {
    // indikerar vilket dokumentavsnitt dokumenttypen ska hamna under
    // Avsnitt:
    // 0 = Planhandling
    // 1 = Övrigt plandokument
    // 2 = Dokumenttyp utan matchande dokument
    dt.Avsnitt = 2;
    // indikerar om dokumenttypen behöver gruppera flera dokument
    dt.Dokumenttypsgrupp = false;
    // Antal deldokument med samma dokumenttyp och som behöver grupperas under dokumenttypen som rubrik
    dt.DokumenttypsdelarAntal = 0;
    // Dokumenttyp förekommer både som delat dokument och odelat - brist. Dokument som delats i dokumentnamnet med delningstecken men som inte fått någon benämning (delnamn) i 2:a delen.
    dt.DokumenttypDelatOdelat = false;
    // Antal dokument per filformat och dokumenttyp
    dt.DokumenttypFormatAntal = [];
  });

  /* =========================
     Flatten all documents
     ========================= */
  const allDocs = plansDocs.flatMap(p => p.documents);

  /* =========================
     Count document parts and mark if needs to be grouped
     ========================= */
  allDocs.forEach(doc => {
    documentTypes.forEach(dt => {
      if (doc.findType === "IsPart" && doc.documentType === dt.type) {
        dt.DokumenttypsdelarAntal++;
      }
    });
  });

  documentTypes.forEach(dt => {
    if (dt.DokumenttypsdelarAntal > 1) {
      dt.Dokumenttypsgrupp = true;
    }
  });

  /* =========================
     Lists
     ========================= */
  // Planhandlingar
  const ulPlanhandling = document.createElement("ul");
  ulPlanhandling.style.marginLeft = "1em"
  // övriga plandokument
  const ulOvriga = document.createElement("ul");
  ulOvriga.style.marginLeft = "1em"
  // Plandokumentstyper utan dokument
  const ulNoDoc = document.createElement("ul");
  ulOvriga.className = "ovrPlandok";


  /* =========================
     Build document items
     ========================= */
  arrayPlanhandlingItems = [];
  arrayOvrPlandokItems = [];
  allDocs.forEach(doc => {

    // Dokumentets dokumenttyp, om existerar, annars kan dokumentet ej användas
    const doctype = documentTypes.find(d => d.type === doc.documentType);
    if (!doctype) return;

    const li = document.createElement("li");
    li.classList.add(doc.extension.substring(1) + "-file");

    const checkbox = document.createElement("input");
    checkbox.type = "checkbox";
    checkbox.addEventListener("change", function() {
      const ulDocs = document.querySelectorAll(".plan-documents-content input[type='checkbox']");
      const total = ulDocs.length;
      const checkedCount = Array.from(ulDocs).filter(cb => cb.checked).length;

      const zipTools = document.querySelector(".planContent-tools-checkzip input[type='checkbox']");

      if (checkedCount === 0) {
        zipTools.indeterminate = false;
      } else if (checkedCount === total) {
        zipTools.indeterminate = false;
      } else {
        zipTools.indeterminate = true;
        if (this.checked) {
          zipTools.title = "Markera alla dokument";
        }
        else {
          zipTools.title = "Avmarkera alla dokument";
        }
      }
    });

    const link = document.createElement("a");
    link.href = `${urlBasePath}/${doc.pathVirtual}/${doc.name}`;
    link.setAttribute("relhref", `${doc.pathVirtual}/${doc.name}`);
    link.target = "_blank";
    link.textContent = doctype.Dokumenttypsgrupp
      ? `${doc.documentPart || "[SAKNAS DELTEXT]"} (${bytesToSize(doc.size)})`
      : `${doc.documentType} (${bytesToSize(doc.size)})`;

    if (doc.documentType == "Karta" && doc.extension == ".tif") {
      let thumnailsImgParam;
      if (doc.thumbnailIndication.includes("s")) {
        //TODO: title fungerar inte för popover och större plankarta
        thumnailsImgParam = '"' + urlBasePath + '/' + doc.thumbnailPath + '/' + doc.name.replace(/\.[^/.]+$/, "") + '_thumnail-s.jpg" /><span class="popover-content-big-image" title="Större bild" aria-label="Större bild"></span>'; //<span class="popover-content-big-image" title="Större bild" aria-label="Större bild"></span>
      }
      else {
        thumnailsImgParam = '"' + urlBasePath + '/images/no-image.png" alt="Avsaknad plankarta som miniatyr" title="Ingen miniatyrbild av plankartan" />';
      }

      const $link = $(link);

      // Ansluter Popover
      $link.popover({
        trigger: 'manual',
        placement: 'auto',
        title: doc.name + ' innehåller',
        html: true,
        content:
          '<div><img src=' + thumnailsImgParam + '</div>'
      }).on('mouseenter', function () {
        var _this = this;
        $(this).popover('show');
        $('.popover').on('mouseleave', function () {
          $(_this).popover('hide');
        });
        $('.popover').find('span').on('click', function () {

          $('<div class="modal" tabindex="-1"><div class="modal-dialog modal-xl modal-wide modal-dialog-centered" style="min-width: 90%"><div class="modal-content"><img style="width:100%;" src="' + urlBasePath + '/' + doc.thumbnailPath + '/' + doc.name.replace(/\.[^/.]+$/, "") + '_thumnail-l.jpg" /></div></div></div>').modal({
            keyboard: true
          });

          $(this).closest('.popover').popover('hide');
        });
      }).on('mouseleave', function () {
        var _this = this;
        setTimeout(function () {
          if (!$('.popover:hover').length) {
            $(_this).popover('hide');
          }
        }, 200);
      });

    }

    li.append(checkbox, link);

    // Fyller arrays för senare avgörande om dokument ska grupperas i GUI under dokumenttyp
    let doctypeSection = {};
    doctypeSection.LiItem = [];
    if (doctype.isPlanhandling) {
      doctype.Avsnitt = 0;
      // Skapar upp 1:a objektet, därefter fylls på
      if (arrayPlanhandlingItems.length == 0) {
        doctypeSection.Name = doctype.type
        doctypeSection.LiItem.push(li);
        arrayPlanhandlingItems.push({ doctypeSection: doctypeSection });
      }
      else {
        // Indikerar om dokumentet inte har hanterats som dokumenttypsdel. Läggs till senare som "vanligt" dokument i så fall.
        let isLiAdded = false;

        // Om dokumentet ska renderas som dokumenttypsdel enl. backend matchning efter namnkonvention
        if (doctype.Dokumenttypsgrupp) {
            // Itterera igenom alla tidigare dokument för gruppering tillsammans efter dokumenttyp
            arrayPlanhandlingItems.forEach((item) => {
                if (item.doctypeSection.Name == doctype.type) {
                    item.doctypeSection.LiItem.push(li);
                    isLiAdded = true;
                }
            });
        }

        // Adderar dokument som ej tidigare adderats
        if (!isLiAdded) {
            doctypeSection.Name = doctype.type;
            doctypeSection.LiItem.push(li);
            arrayPlanhandlingItems.push({ doctypeSection: doctypeSection });
        }
      }
    } else {
      doctype.Avsnitt = 1;
      // Skapar upp 1:a objektet, därefter fylls på
      if (arrayOvrPlandokItems.length == 0) {
        doctypeSection.Name = doctype.type
        doctypeSection.LiItem.push(li);
        arrayOvrPlandokItems.push({ doctypeSection: doctypeSection });
      }
      else {
        // Indikerar om dokumentet inte har hanterats som dokumenttypsdel. Läggs till senare som "vanligt" dokument i så fall.
        let isLiAdded = false;

        // Om dokumentet ska renderas som dokumenttypsdel enl. backend matchning efter namnkonvention
        if (doctype.Dokumenttypsgrupp) {
            // Itterera igenom alla tidigare dokument för gruppering tillsammans efter dokumenttyp
            arrayOvrPlandokItems.forEach((item) => {
                if (item.doctypeSection.Name == doctype.type) {
                    item.doctypeSection.LiItem.push(li);
                    isLiAdded = true;
                }
            });
        }

        // Adderar dokument som ej tidigare adderats
        if (!isLiAdded) {
            doctypeSection.Name = doctype.type;
            doctypeSection.LiItem.push(li);
            arrayOvrPlandokItems.push({ doctypeSection: doctypeSection });
        }
      }
    }
  });
  
  // Gruppering av GUI under dokumenttyp
  arrayPlanhandlingItems.forEach((item) => {
    // Om flera listobjekt, gruppera dessa under dokumenttypsrubrik, annars addera direkt till lista
    if (item.doctypeSection.LiItem.length > 1) {
      const liGroup = document.createElement("li");
      liGroup.style.padding = 0;
      liGroup.innerText = `${item.doctypeSection.Name} (dokument uppdelade med samma dokumenttyp)`
      const olGroup = document.createElement("ol");
      olGroup.style.paddingLeft = "24px";
      // Bygg grupplista för alla listobjekt av dokumenttypen
      item.doctypeSection.LiItem.forEach((liItem) => {
          olGroup.append(liItem);
      });
      liGroup.append(olGroup);
      ulPlanhandling.appendChild(liGroup);
    }
    else {
      ulPlanhandling.appendChild(item.doctypeSection.LiItem[0]);
    }
  });
  arrayOvrPlandokItems.forEach((item) => {
        // Om flera listobjekt, gruppera dessa under dokumenttypsrubrik, annars addera direkt till lista
    if (item.doctypeSection.LiItem.length > 1) {
      const liGroup = document.createElement("li");
      liGroup.style.padding = 0;
      liGroup.innerText = `${item.doctypeSection.Name} (dokument med samma dokumenttyp avsnittsuppdelade)`
      const olGroup = document.createElement("ol");
      olGroup.style.paddingLeft = "24px";
      // Bygg grupplista för alla listobjekt av dokumenttypen
      item.doctypeSection.LiItem.forEach((liItem) => {
          olGroup.append(liItem);
      });
      liGroup.append(olGroup);
      ulOvriga.appendChild(liGroup);
    }
    else {
      ulOvriga.appendChild(item.doctypeSection.LiItem[0]);
    }
  });


  // Hantera dokumenttyper som ej har några dokument 
  const doctypeNoDoc = documentTypes.filter(d => d.Avsnitt === 2 && d.type.trim() !== '');

  const noDocsWrapper = document.createElement("div");
  if (doctypeNoDoc) {
    
    // fyller ulNoDoc med dokumenttyper som inte har något motsvarande dokument
    doctypeNoDoc.forEach(d => {
      const li = document.createElement("li");
      li.classList.add("no-file");
      li.textContent = d.type;
      ulNoDoc.appendChild(li);
    });

    const noDocsHeader = document.createElement("div");
    noDocsHeader.className = "docs";
    noDocsHeader.title = "Klicka för att expandera";

    const titleSpan = document.createElement("span");
    titleSpan.className = "planContent-avsnitt";
    titleSpan.textContent = "Ej enskilt upprättade dokument";

    const icon = document.createElement("div");
    icon.className = "docs-collapsed";
    icon.style.marginLeft = "0";
    icon.style.display = "inline-block";

    const dots = document.createElement("span");
    dots.textContent = "...";

    noDocsHeader.append(titleSpan, document.createElement("br"), icon, dots);

    const listWrapper = document.createElement("div");
    listWrapper.style.marginLeft = "1em";
    listWrapper.style.marginTop = "-1em";
    listWrapper.style.display = "none";

    if (ulNoDoc.children.length > 0) {
      listWrapper.appendChild(ulNoDoc);
    } else {
      listWrapper.append(document.createElement("br"), document.createTextNode("-"));
    }


    noDocsHeader.addEventListener("mouseenter", () => {
      icon.classList.add(
        listWrapper.style.display === "block"
          ? "docs-expand-hover"
          : "docs-collapsed-hover"
      );
    });

    noDocsHeader.addEventListener("mouseleave", () => {
      icon.classList.remove("docs-expand-hover", "docs-collapsed-hover");
      icon.classList.add(
        listWrapper.style.display === "block"
          ? "docs-expand"
          : "docs-collapsed"
      );
    });


    noDocsHeader.addEventListener("click", () => {
      const expanded = listWrapper.style.display === "block";

      listWrapper.style.display = expanded ? "none" : "block";

      icon.classList.remove(
        "docs-expand",
        "docs-expand-hover",
        "docs-collapsed",
        "docs-collapsed-hover"
      );

      icon.classList.add(expanded ? "docs-collapsed" : "docs-expand");
      dots.textContent = expanded ? "..." : "";
    });

    noDocsWrapper.append(noDocsHeader, listWrapper);
  }

  /* =========================
     Append sections
     ========================= */
  const planhandlingHeader = document.createElement("span");
  planhandlingHeader.className = "planContent-avsnitt";
  planhandlingHeader.textContent = "Planhandlingar";
  content.append(planhandlingHeader);

  if (ulPlanhandling.children.length) {
    content.appendChild(ulPlanhandling);
  } else {
    content.append(document.createElement("br"), document.createTextNode("-"), document.createElement("br"));
  }

  const ovrHeader = document.createElement("span");
  ovrHeader.className = "planContent-avsnitt";
  ovrHeader.textContent = "Övriga plandokument";
  content.append(ovrHeader);

  if (ulOvriga.children.length) {
    content.appendChild(ulOvriga);
  } else {
    content.append(document.createElement("br"), document.createTextNode("-"));
  }

  content.append(noDocsWrapper);

  /* =========================
     Toolbar
     ========================= */
  if (allDocs) {

    const toolbarPackContainer = document.createElement("div");
    const toolbarPackFunctionBtn = document.createElement("div");
    toolbarPackFunctionBtn.title = "Markera filer och ladda ner";
    toolbarPackFunctionBtn.className = "planContent-tools-zip";
    toolbarPackFunctionBtn.addEventListener("click", () => {
      toggleToolZip(toolbarPackContainer, content)
    });
    const toolbarPackFunctionBtnClickArea = document.createElement("div");
    toolbarPackFunctionBtnClickArea.className = "clickArea";
    toolbarPackFunctionBtn.append(toolbarPackFunctionBtnClickArea);
    const toolbarPack = document.createElement("div");
    toolbarPack.className = "planContent-tools-checkzip"


    const selectAll = document.createElement("input");
    selectAll.type = "checkbox";
    selectAll.title = "Markera alla dokument";

    selectAll.addEventListener("change", () => {
      if (selectAll.checked) {
        selectAll.title = "Avmarkera alla dokument";
      }
      else {
        selectAll.title = "Markera alla dokument";
      }
      content.querySelectorAll("ul input[type=checkbox]")
        .forEach(cb => cb.checked = selectAll.checked);
    });

    const packBtn = document.createElement("img");
    packBtn.src = `${urlBasePath}/images/noun_4501_16x16_black.png`;
    packBtn.title = "Paketera valda filer och ladda ned";
    packBtn.className = "hand imgPlanDokument";

    packBtn.addEventListener("click", () => {
      const files = [...content.querySelectorAll("ul input:checked")]
        .map(cb => cb.nextSibling.getAttribute("relhref"));

      if (!files.length) {
        alert("Inga dokument valda!");
        return;
      }

      // Hämtar aktbeteckning som filnamn för zip-fil
      const r = document.getElementById("DocumentRow");
      let zipFileName
      if (r) { 
        zipFileName = r.previousSibling.cells[2].querySelector("a").firstChild.textContent;
        // Avgör om alla eller delar av planens dokument är valda (tiff-filer ej inräknade)
        const excludedExtensions = [".tif", ".tiff"];
        if (files.filter(file =>
            !excludedExtensions.some(ext =>
              file.toLowerCase().endsWith(ext)
            )).length ==
          plansDocs[0].documents.filter(doc => !excludedExtensions.includes(doc.extension.toLowerCase())).length
        ) {
          zipFileName += "_alla_plandokument"
        }
        else {
          zipFileName += "_delar_av_plandokumenten"
        }
      }
      else {
        zipFileName = "plandokument";
        // Avgör om alla eller delar av planens dokument är valda (tiff-filer ej inräknade)
        const excludedExtensions = [".tif", ".tiff"];
        if (files.filter(file =>
            !excludedExtensions.some(ext =>
              file.toLowerCase().endsWith(ext)
            )).length ==
          plansDocs[0].documents.filter(doc => !excludedExtensions.includes(doc.extension.toLowerCase())).length
        ) {
          zipFileName += "_alla_plandokument"
        }
        else {
          zipFileName += "_delar_av_plandokumenten"
        }
      }

      getFilesZipped(files, zipFileName);
    });

    toolbarPack.append(selectAll, packBtn);
    toolbarPackContainer.append(toolbarPackFunctionBtn, toolbarPack);

    toolbar.append(toolbarPackContainer);
  }

  return root;
}


function toggleToolZip(toolsContainer, docsContainer) {
    const zipTools = toolsContainer.querySelector(".planContent-tools-checkzip");
    if (!zipTools) return;

    const isOpen = zipTools.classList.toggle("open");

    docsContainer
        .querySelectorAll("ul input[type=checkbox]")
        .forEach(cb => cb.style.display = isOpen ? "inline-block" : "none");
}


async function getFilesZipped(files, fileNamePart) {
    // toggleLoadingImage(true);

    try {
        const response = await fetch(
            urlBasePath + "/api/Compression/zip/create",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json; charset=UTF-8"
                },
                body: JSON.stringify({
                    documentPaths: files,
                    baseName: fileNamePart
                })
            }
        );

        const result = await response.json();
        if (result) {
          location.href = result.zipPath;
        }
        else
        {
          throw new Error("zip-response gav ett tomt svar");
        }
    } catch (err) {
        alert("Fel!\ngetFilesZipped");
        console.error(err);
    } finally {
        // toggleLoadingImage(false);
    }
}
