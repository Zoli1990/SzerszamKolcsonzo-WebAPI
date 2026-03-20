<template>
  <div class="aszf-page">
    <div class="aszf-container">
      <header class="aszf-header">
        <h1>📋 Általános Szerződési Feltételek</h1>
        <p class="aszf-subtitle">Szerszámkölcsönző – Online Foglalási Rendszer</p>
        <p class="aszf-date">Hatályos: 2025. január 1-től</p>
      </header>

      <nav class="aszf-nav">
        <a v-for="section in sections" :key="section.id" :href="'#' + section.id" class="aszf-nav-link">
          {{ section.label }}
        </a>
      </nav>

      <div class="aszf-content">

        <!-- 1. -->
        <section id="altalanos" class="aszf-section">
          <h2>1. Általános rendelkezések</h2>
          <p>
            Jelen Általános Szerződési Feltételek (a továbbiakban: ÁSZF) a <strong>Szerszámkölcsönző</strong>
            (a továbbiakban: Szolgáltató) és az online foglalási rendszert igénybe vevő természetes
            személyek (a továbbiakban: Ügyfél) között létrejövő kölcsönzési jogviszony feltételeit
            rögzíti.
          </p>
          <p>
            Az online foglalás leadásával az Ügyfél kijelenti, hogy jelen ÁSZF-et megismerte,
            megértette és magára nézve kötelezőnek fogadja el.
          </p>
        </section>

        <!-- 2. -->
        <section id="foglalas-rendszer" class="aszf-section">
          <h2>2. Az online foglalási rendszer célja és működése</h2>
          <p>
            Az online foglalási rendszer célja, hogy az Ügyfelek előzetesen jelezzék igényüket
            egy adott eszköz kölcsönzésére. <strong>A foglalás egy szándéknyilatkozat</strong>,
            amely az eszköz tényleges kiadását nem garantálja automatikusan — a kiadáshoz
            személyes megjelenés szükséges.
          </p>
          <ul>
            <li>Az eszközök státuszát kizárólag a Szolgáltató munkatársai módosítják.</li>
            <li>Egy eszközre naponta több, egymást nem fedő foglalás is leadható.</li>
            <li>A foglalás leadása nem jelent azonnali kiadást — az eszközt az üzletben kell átvenni.</li>
            <li>
              Az Ügyfél felelőssége, hogy az eszköz aktuális elérhetőségi státuszát
              megjelenés előtt ellenőrizze (a weboldalon, telefonon vagy e-mailben).
            </li>
          </ul>
        </section>

        <!-- 3. -->
        <section id="foglalas-menete" class="aszf-section">
          <h2>3. A foglalás menete</h2>
          <ol>
            <li>Az Ügyfél kiválasztja a kívánt eszközt és a kívánt időintervallumot (tól–ig).</li>
            <li>Megadja nevét, e-mail-címét, telefonszámát és lakcímét.</li>
            <li>Elfogadja jelen ÁSZF-et, majd elküldi a foglalási kérelmet.</li>
            <li>
              A rendszer megvizsgálja, hogy a kiválasztott időintervallum ütközik-e meglévő
              foglalással. Ütközés esetén a foglalás elutasításra kerül.
            </li>
            <li>Sikeres foglalás esetén az Ügyfél értesítést kap a profil oldalán.</li>
          </ol>
          <div class="aszf-callout info">
            <span class="callout-icon">📬</span>
            <span>
              Az esetleges státuszváltozásokról (pl. az eszköz előre visszahozatala miatti
              korábbi elérhetővé válás) az Ügyfél a <strong>„Foglalásaim"</strong> oldalon
              kap valós idejű értesítést, ha bejelentkezett állapotban böngészi az oldalt.
            </span>
          </div>
        </section>

        <!-- 4. -->
        <section id="megjelenes" class="aszf-section">
          <h2>4. Megjelenési kötelezettség és a 15 perces szabály</h2>
          <p>
            Az Ügyfélnek a foglalt időpontra pontosan meg kell jelennie az üzletben.
          </p>
          <div class="aszf-callout warning">
            <span class="callout-icon">⏱️</span>
            <span>
              <strong>Ha az Ügyfél a foglalt kezdési időponttól számított 15 percen belül nem jelenik meg,</strong>
              a rendszer automatikusan törli a foglalást, és az időszak más ügyfelek számára
              újra elérhetővé válik.
            </span>
          </div>
          <p>
            Késés esetén javasolt telefonon értesíteni a Szolgáltatót, aki mérlegelési
            jogkörrel bír a foglalás fenntartásáról.
          </p>
        </section>

        <!-- 5. -->
        <section id="kiadas" class="aszf-section">
          <h2>5. Az eszköz kiadása</h2>
          <p>
            A Szolgáltató <strong>nem köteles kiadni az eszközt</strong>, ha:
          </p>
          <ul>
            <li>az eszköz időközben meghibásodott, karbantartás alatt áll vagy nem elérhető;</li>
            <li>az Ügyfél érvényes személyazonosító okmányt nem tud felmutatni;</li>
            <li>az Ügyfél a korábbi kölcsönzések során kárt okozott vagy egyéb kötelezettségét nem teljesítette;</li>
            <li>az Ügyfél a Szolgáltató megítélése szerint nem képes az eszközt rendeltetésszerűen használni.</li>
          </ul>
          <div class="aszf-callout info">
            <span class="callout-icon">✅</span>
            <span>
              <strong>Javasolt az eszköz státuszát a megjelenés előtt ellenőrizni</strong>
              a weboldalon, telefonon (+36 30 XXX XXXX) vagy e-mailben
              (info@szerszamkolcsonzo.hu), hogy ne tegye meg hiába az utat.
            </span>
          </div>
        </section>

        <!-- 6. -->
        <section id="dij" class="aszf-section">
          <h2>6. A kölcsönzési díj</h2>
          <p>
            A kölcsönzési díj az eszköz adatlapján feltüntetett óradíj alapján kerül kiszámításra,
            a kölcsönzési időintervallum (tól–ig) alapján. A foglalás leadásakor megjelenő
            becsült díj tájékoztató jellegű; a végleges összeget a kiadáskor a Szolgáltató
            közli.
          </p>
          <ul>
            <li>A díj kifizetése az eszköz átvételekor esedékes, készpénzben vagy bankkártyával.</li>
            <li>Visszamondott vagy le nem vett foglalás esetén díj nem kerül felszámításra.</li>
            <li>
              <strong>Késedelmes visszahozatal esetén</strong> a késedelemmel érintett minden
              megkezdett óra után az óradíj 1,5-szöröse kerül felszámításra, amennyiben az eszközre
              ezt követő foglalás érkezett.
            </li>
          </ul>
        </section>

        <!-- 7. -->
        <section id="visszahozatal" class="aszf-section">
          <h2>7. Visszahozatali kötelezettség</h2>
          <p>
            Az Ügyfél köteles az eszközt a foglalásban rögzített befejezési időpontig
            visszaszolgáltatni a Szolgáltatónak.
          </p>
          <ul>
            <li>Az eszközt tisztán, az eredeti állapotában és tartozékaival együtt kell visszahozni.</li>
            <li>
              Ha az Ügyfél az eszközt hosszabb ideig kívánja tartani, ezt <strong>előzetesen
              egyeztetni kell</strong> a Szolgáltatóval, és csak szabad időszakra lehetséges.
            </li>
            <li>Előre nem bejelentett késedelem esetén a Szolgáltató pótdíjat számíthat fel.</li>
          </ul>
        </section>

        <!-- 8. -->
        <section id="karfelel" class="aszf-section">
          <h2>8. Kárfelelősség</h2>
          <p>
            Az Ügyfél teljes körű anyagi felelősséggel tartozik az általa okozott károkért,
            beleértve:
          </p>
          <ul>
            <li>az eszköz megrongálódását, törését vagy elvesztését;</li>
            <li>a nem rendeltetésszerű használatból eredő hibákat;</li>
            <li>tartozékok, kiegészítők elvesztését.</li>
          </ul>
          <p>
            A kárt a Felek kölcsönös megállapodással vagy független szakértő bevonásával
            értékelik. Az Ügyfél a kár értékét köteles megtéríteni legkésőbb az eszköz
            visszahozatalakor.
          </p>
        </section>

        <!-- 9. -->
        <section id="adatkezeles" class="aszf-section">
          <h2>9. Adatkezelés</h2>
          <p>
            A foglalás során megadott személyes adatokat (név, e-mail, telefonszám, cím)
            a Szolgáltató kizárólag a kölcsönzési jogviszony teljesítése céljából kezeli,
            harmadik félnek nem adja át, és a kölcsönzés befejezését követő 1 éven belül törli.
          </p>
          <p>
            Az adatkezelés jogalapja az Ügyfél hozzájárulása és a szerződés teljesítése
            (GDPR 6. cikk (1) b) és a) pont).
          </p>
        </section>

        <!-- 10. -->
        <section id="egyeb" class="aszf-section">
          <h2>10. Egyéb rendelkezések</h2>
          <ul>
            <li>A Szolgáltató fenntartja a jogot jelen ÁSZF egyoldalú módosítására. A módosítás a weboldalon való közzététellel lép hatályba.</li>
            <li>A jelen ÁSZF-ben nem szabályozott kérdésekben a Ptk. (2013. évi V. törvény) és a vonatkozó magyar jogszabályok az irányadók.</li>
            <li>Vitás esetekben a Felek elsősorban tárgyalásos úton törekednek megegyezésre.</li>
          </ul>
        </section>

      </div>

      <div class="aszf-back">
        <button class="btn-back" @click="$router.back()">← Vissza</button>
      </div>
    </div>
  </div>
</template>

<script setup>
const sections = [
  { id: 'altalanos', label: '1. Általános rendelkezések' },
  { id: 'foglalas-rendszer', label: '2. A foglalási rendszer' },
  { id: 'foglalas-menete', label: '3. Foglalás menete' },
  { id: 'megjelenes', label: '4. 15 perces szabály' },
  { id: 'kiadas', label: '5. Eszköz kiadása' },
  { id: 'dij', label: '6. Kölcsönzési díj' },
  { id: 'visszahozatal', label: '7. Visszahozatal' },
  { id: 'karfelel', label: '8. Kárfelelősség' },
  { id: 'adatkezeles', label: '9. Adatkezelés' },
  { id: 'egyeb', label: '10. Egyéb' },
]
</script>

<style scoped>
.aszf-page {
  background: var(--color-background);
  min-height: 100vh;
  padding: var(--spacing-xl) var(--spacing-md);
}

.aszf-container {
  max-width: 860px;
  margin: 0 auto;
}

/* ── HEADER ── */
.aszf-header {
  text-align: center;
  margin-bottom: var(--spacing-xl);
  padding-bottom: var(--spacing-lg);
  border-bottom: 2px solid var(--color-border);
}

.aszf-header h1 {
  font-size: 2rem;
  color: var(--color-text);
  margin-bottom: var(--spacing-sm);
}

.aszf-subtitle {
  color: var(--color-text-muted);
  font-size: 1.05rem;
  margin-bottom: 4px;
}

.aszf-date {
  color: var(--color-text-muted);
  font-size: 0.875rem;
}

/* ── NAV ── */
.aszf-nav {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-xl);
  background: var(--color-surface);
  padding: var(--spacing-md);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

.aszf-nav-link {
  font-size: 0.8rem;
  color: var(--color-primary);
  text-decoration: none;
  padding: 4px 10px;
  border-radius: var(--radius-sm);
  border: 1px solid var(--color-primary);
  transition: all var(--transition-fast);
  white-space: nowrap;
}

.aszf-nav-link:hover {
  background: var(--color-primary);
  color: white;
}

/* ── SECTIONS ── */
.aszf-content {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xl);
}

.aszf-section {
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg) var(--spacing-xl);
  box-shadow: var(--shadow-sm);
}

.aszf-section h2 {
  font-size: 1.2rem;
  color: var(--color-primary-dark);
  margin-bottom: var(--spacing-md);
  padding-bottom: var(--spacing-sm);
  border-bottom: 1px solid var(--color-border);
}

.aszf-section p {
  color: var(--color-text);
  line-height: 1.7;
  margin-bottom: var(--spacing-md);
}

.aszf-section p:last-child {
  margin-bottom: 0;
}

.aszf-section ul,
.aszf-section ol {
  padding-left: var(--spacing-lg);
  margin-bottom: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.aszf-section li {
  color: var(--color-text);
  line-height: 1.6;
}

/* ── CALLOUTS ── */
.aszf-callout {
  display: flex;
  gap: var(--spacing-md);
  align-items: flex-start;
  padding: var(--spacing-md);
  border-radius: var(--radius-md);
  margin: var(--spacing-md) 0;
  line-height: 1.6;
  font-size: 0.95rem;
}

.aszf-callout.info {
  background: #e0f2fe;
  border-left: 4px solid #0ea5e9;
  color: #0c4a6e;
}

.aszf-callout.warning {
  background: #fef3c7;
  border-left: 4px solid #f59e0b;
  color: #78350f;
}

.callout-icon {
  font-size: 1.4rem;
  flex-shrink: 0;
  margin-top: 2px;
}

/* ── BACK BUTTON ── */
.aszf-back {
  margin-top: var(--spacing-xl);
  text-align: center;
}

.btn-back {
  padding: var(--spacing-sm) var(--spacing-xl);
  background: var(--color-primary);
  color: white;
  border: none;
  border-radius: var(--radius-md);
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background var(--transition-fast);
}

.btn-back:hover {
  background: var(--color-primary-dark);
}

@media (max-width: 600px) {
  .aszf-header h1 {
    font-size: 1.4rem;
  }

  .aszf-section {
    padding: var(--spacing-md);
  }

  .aszf-nav {
    gap: 6px;
  }
}
</style>
