<template>
  <div class="kapcsolat-section">
    <div class="container">
      <h2 class="section-title">📍 Látogass el hozzánk!</h2>
      
      <div class="content-wrapper">
        <!-- BAL OLDAL: Térkép -->
        <div class="map-container">
          <iframe
            src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d2757.8!2d20.1482!3d46.2530!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x4744870e5c0e8e7d%3A0x5c5e5c5e5c5e5c5e!2sSzeged%2C%20Lidicei%20t%C3%A9r%201!5e0!3m2!1shu!2shu!4v1234567890"
            width="100%"
            height="100%"
            style="border:0; border-radius: 12px;"
            allowfullscreen=""
            loading="lazy"
            referrerpolicy="no-referrer-when-downgrade"
            title="Üzlet helyszíne - Szeged, Lidicei tér 1"
          ></iframe>
        </div>

        <!-- JOBB OLDAL: Üzlet információk -->
        <div class="info-container">
          <!-- CÍM -->
          <div class="info-card">
            <div class="icon">📍</div>
            <div class="info-content">
              <h3>Címünk</h3>
              <p class="info-text">Szeged, Lidicei tér 1</p>
              <p class="info-subtext">6724 Magyarország</p>
            </div>
          </div>

          <!-- NYITVATARTÁS -->
          <div class="info-card">
            <div class="icon">🕐</div>
            <div class="info-content">
              <h3>Nyitvatartás</h3>
              <div class="opening-hours">
                <div class="day-row">
                  <span class="day">Hétfő</span>
                  <span class="hours">08:00 - 20:00</span>
                </div>
                <div class="day-row">
                  <span class="day">Kedd</span>
                  <span class="hours">08:00 - 20:00</span>
                </div>
                <div class="day-row">
                  <span class="day">Szerda</span>
                  <span class="hours">08:00 - 20:00</span>
                </div>
                <div class="day-row">
                  <span class="day">Csütörtök</span>
                  <span class="hours">08:00 - 20:00</span>
                </div>
                <div class="day-row">
                  <span class="day">Péntek</span>
                  <span class="hours">08:00 - 20:00</span>
                </div>
                <div class="day-row">
                  <span class="day">Szombat</span>
                  <span class="hours">08:00 - 20:00</span>
                </div>
                <div class="day-row">
                  <span class="day">Vasárnap</span>
                  <span class="hours">08:00 - 20:00</span>
                </div>
              </div>
            </div>
          </div>

          <!-- ELÉRHETŐSÉGEK -->
          <div class="info-card">
            <div class="icon">📞</div>
            <div class="info-content">
              <h3>Elérhetőségek</h3>
              <p class="info-text">
                <a href="tel:+36301234567" class="contact-link">+36 30 123 4567</a>
              </p>
              <p class="info-text">
                <a href="mailto:info@szerszamkolcsonzo.hu" class="contact-link">info@szerszamkolcsonzo.hu</a>
              </p>
            </div>
          </div>

          <!-- ÚTBAIGAZÍTÁS GOMB -->
          <a 
            href="https://www.google.com/maps/dir/?api=1&destination=Szeged,+Lidicei+tér+1" 
            target="_blank" 
            rel="noopener noreferrer"
            class="directions-btn"
          >
            🧭 Útvonaltervezés
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
// Nincs szükség script logikára ebben a statikus komponensben
</script>

<style scoped>
.kapcsolat-section {
  padding: 80px 20px;
  background: linear-gradient(135deg, #f5f1e8 0%, #e8dcc8 100%);
  position: relative;
  overflow: hidden;
}

/* Dekoratív háttér minta */
.kapcsolat-section::before {
  content: '';
  position: absolute;
  top: -50%;
  right: -10%;
  width: 500px;
  height: 500px;
  background: radial-gradient(circle, rgba(107, 142, 35, 0.08) 0%, transparent 70%);
  border-radius: 50%;
  pointer-events: none;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  position: relative;
  z-index: 1;
}

.section-title {
  text-align: center;
  font-size: 36px;
  color: #3d2f1f;
  margin-bottom: 48px;
  font-weight: 700;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.05);
}

.content-wrapper {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 32px;
  align-items: start;
}

/* ============================================================================ */
/* TÉRKÉP STÍLUS */
/* ============================================================================ */
.map-container {
  height: 600px;
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 8px 24px rgba(61, 47, 31, 0.15);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.map-container:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 32px rgba(61, 47, 31, 0.2);
}

/* ============================================================================ */
/* INFORMÁCIÓS KÁRTYÁK */
/* ============================================================================ */
.info-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.info-card {
  background: white;
  padding: 24px;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(61, 47, 31, 0.1);
  display: flex;
  gap: 20px;
  align-items: flex-start;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.info-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(61, 47, 31, 0.15);
}

.icon {
  font-size: 36px;
  min-width: 50px;
  text-align: center;
  background: linear-gradient(135deg, #f5f1e8 0%, #e8dcc8 100%);
  padding: 12px;
  border-radius: 12px;
}

.info-content {
  flex: 1;
}

.info-content h3 {
  margin: 0 0 12px 0;
  font-size: 20px;
  color: #3d2f1f;
  font-weight: 700;
}

.info-text {
  margin: 0 0 8px 0;
  font-size: 16px;
  color: #3d2f1f;
  font-weight: 600;
}

.info-subtext {
  margin: 0;
  font-size: 14px;
  color: #6b5d4f;
}

.contact-link {
  color: #6b8e23;
  text-decoration: none;
  font-weight: 600;
  transition: color 0.2s;
}

.contact-link:hover {
  color: #556b1a;
  text-decoration: underline;
}

/* ============================================================================ */
/* NYITVATARTÁS TÁBLÁZAT */
/* ============================================================================ */
.opening-hours {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.day-row {
  display: flex;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid #e8dcc8;
}

.day-row:last-child {
  border-bottom: none;
}

.day {
  font-weight: 600;
  color: #3d2f1f;
  font-size: 15px;
}

.hours {
  color: #6b8e23;
  font-weight: 700;
  font-size: 15px;
}

/* ============================================================================ */
/* ÚTVONALTERVEZÉS GOMB */
/* ============================================================================ */
.directions-btn {
  display: block;
  width: 100%;
  padding: 18px 32px;
  background: linear-gradient(135deg, #6b8e23 0%, #556b1a 100%);
  color: white;
  text-align: center;
  text-decoration: none;
  font-weight: 700;
  font-size: 18px;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(107, 142, 35, 0.3);
  transition: all 0.3s ease;
  margin-top: 8px;
}

.directions-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(107, 142, 35, 0.4);
  background: linear-gradient(135deg, #556b1a 0%, #445516 100%);
}

.directions-btn:active {
  transform: translateY(0);
  box-shadow: 0 2px 8px rgba(107, 142, 35, 0.3);
}

/* ============================================================================ */
/* RESZPONZÍV DESIGN */
/* ============================================================================ */
@media (max-width: 968px) {
  .content-wrapper {
    grid-template-columns: 1fr;
  }

  .map-container {
    height: 400px;
  }

  .section-title {
    font-size: 28px;
  }
}

@media (max-width: 640px) {
  .kapcsolat-section {
    padding: 60px 16px;
  }

  .section-title {
    font-size: 24px;
    margin-bottom: 32px;
  }

  .info-card {
    padding: 20px;
    gap: 16px;
  }

  .icon {
    font-size: 28px;
    min-width: 40px;
    padding: 10px;
  }

  .info-content h3 {
    font-size: 18px;
  }

  .info-text {
    font-size: 14px;
  }

  .day-row {
    font-size: 14px;
  }

  .directions-btn {
    font-size: 16px;
    padding: 16px 24px;
  }
}
</style>