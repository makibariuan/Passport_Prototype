<template>
  <div class="page" v-if="app">

    <!-- TOP BAR -->
    <header class="topbar animate-fade-down">
      <div>
        <h2>{{ app.applicationCode }}</h2>
        <small>{{ formatDate(app.applicationSchedule) }}</small>
      </div>

      <span class="status" :class="statusClass(app.applicationStatus)">
        {{ getStatus(app.applicationStatus) }}
      </span>
    </header>

    <!-- GRID -->
    <main class="grid">

      <section class="card animate-up">
        <h3>👤 Personal Info</h3>
        <p class="name">{{ fullName(app) }}</p>
        <p class="muted">{{ app.country }} • {{ app.region }}</p>
      </section>

      <section class="card animate-up delay-1">
        <h3>📄 Application</h3>

        <div class="info-grid">
          <div>
            <span class="label">Type</span>
            <span>{{ app.appType }}</span>
          </div>

          <div>
            <span class="label">Site</span>
            <span>{{ app.applicationSite }}</span>
          </div>

          <div>
            <span class="label">Courtesy</span>
            <span>{{ app.applicationCourtesy ? "Yes" : "No" }}</span>
          </div>
        </div>
      </section>

      <section class="card highlight animate-up delay-2">
        <h3>💰 Payment</h3>

        <div class="payment-row">
          <div class="amount">₱{{ app.amount }}</div>

          <span class="pill" :class="app.applicationPaid ? 'paid' : 'unpaid'">
            {{ app.applicationPaid ? "Paid" : "Unpaid" }}
          </span>
        </div>
      </section>

      <!-- DOCUMENTS -->
    <section class="card full animate-up delay-3">
    <h3>📷 Documents</h3>

    <div class="gallery">

        <!-- VALID ID -->
        <div class="doc" @click="openFile(app.validIdPath)">
        <div class="doc-box large hover-zoom">

            <template v-if="isImage(app.validIdPath)">
            <img :src="getImage(app.validIdPath)" />
            </template>

            <template v-else>
            <div class="pdf-preview">
                📄 Open PDF
            </div>
            </template>

        </div>
        <span>Valid ID</span>
        </div>

        <!-- CERTIFICATE -->
        <div class="doc" @click="openFile(app.certificatePath)">
        <div class="doc-box large hover-zoom">

            <template v-if="isImage(app.certificatePath)">
            <img :src="getImage(app.certificatePath)" />
            </template>

            <template v-else>
            <div class="pdf-preview">
                📄 Open PDF
            </div>
            </template>

        </div>
        <span>Certificate</span>
        </div>

    </div>
    </section>

    </main>

    <!-- MODAL -->
    <!-- FILE VIEWER -->
    <transition name="fade">
    <div v-if="viewer" class="viewer" @click="closeViewer">

        <!-- IMAGE VIEW -->
        <img
        v-if="viewer.type === 'image'"
        :src="viewer.url"
        class="viewer-img"
        />

        <!-- PDF VIEW -->
        <iframe
        v-else
        :src="viewer.url"
        class="viewer-pdf"
        ></iframe>

    </div>
    </transition>

  </div>

  <div v-else class="loading animate-pulse">
    Loading application...
  </div>
</template>


<script setup>
import { ref, onMounted } from "vue";
import { useRoute } from "vue-router";

const route = useRoute();

/* =========================
   STATE
========================= */
const app = ref(null);
const loading = ref(true);
const previewImage = ref(null);
const viewer = ref(null) // { type: 'image' | 'pdf', url: '' }


/* =========================
  Document  STATE
========================= */
const isPdf = (path) => {
  if (!path) return false;
  return path.toLowerCase().includes(".pdf");
};

const isImage = (path) => {
  if (!path) return false;
  return /\.(jpg|jpeg|png|webp)$/i.test(path);
};

const openFile = (path) => {
  if (!path) return

  const url = getImage(path)

  if (isPdf(path)) {
    viewer.value = { type: 'pdf', url }
  } else {
    viewer.value = { type: 'image', url }
  }
}

const closeViewer = () => {
  viewer.value = null
}

/* =========================
   FETCH APPLICATION
========================= */
const fetchData = async () => {
  try {
    loading.value = true;

    const code = route.params.code;

    if (!code) {
      console.error("Missing application code in route");
      app.value = null;
      return;
    }

    const res = await fetch(
      `/api/Application/Get_specific_user_applicationinformation?applicationCode=${code}`
    );

    if (!res.ok) {
      console.error("API error:", res.status);
      app.value = null;
      return;
    }

    const data = await res.json();

    // API returns array → take first item
    app.value = Array.isArray(data) ? data[0] : data;

  } catch (err) {
    console.error("API Error:", err);
    app.value = null;
  } finally {
    loading.value = false;
  }
};

/* =========================
   ON LOAD
========================= */
onMounted(fetchData);

/* =========================
   IMAGE/File PREVIEW
========================= */
// const preview = (img) => {
//   previewImage.value = img;
// };

const preview = (path) => {
  if (!path) return;

  // If PDF → open in new tab
  if (isPdf(path)) {
    window.open(path, "_blank");
    return;
  }

  // If image → modal preview
  previewImage.value = path;
};

/* =========================
   HELPERS
========================= */
const fullName = (a) =>
  `${a.passportFirstName || ""} ${a.passportMiddleName || ""} ${a.passportLastName || ""}`.trim();

const formatDate = (date) => {
  if (!date) return "";
  return new Date(date).toLocaleString();
};

const getStatus = (status) => {
  return {
    1: "Pending",
    2: "Approved",
    3: "Rejected",
  }[status] || "Unknown";
};

const statusClass = (status) => [
  "badge",
  status === 1 && "pending",
  status === 2 && "approved",
  status === 3 && "rejected",
];

/* =========================
   IMAGE BASE URL
========================= */
const getImage = (path) => {
  if (!path) return "";
  return `https://localhost:5000${path}`; // change to your real API host
};
</script>

    <style scoped>
    .page {
        padding: 20px;
        background: #f5f7fb;
        min-height: 100vh;
        font-family: system-ui, sans-serif;
        animation: fadeIn 0.4s ease;
    }

    /* TOPBAR */
    .topbar {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 18px 20px;
        background: white;
        border-radius: 14px;
        margin-bottom: 20px;
        box-shadow: 0 10px 25px rgba(0,0,0,0.06);
    }

    /* STATUS PULSE */
    .status {
        padding: 6px 12px;
        border-radius: 999px;
        font-weight: 600;
        animation: pulse 2s infinite;
    }

    .paid { background: #dcfce7; color: #166534; }
    .unpaid { background: #fee2e2; color: #991b1b; }

    /* GRID */
    .grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 16px;
    }

    /* CARD */
    .card {
        background: white;
        border-radius: 14px;
        padding: 18px;
        box-shadow: 0 8px 20px rgba(0,0,0,0.05);
        transition: all 0.25s ease;
        transform: translateY(0);
    }

    .card:hover {
        transform: translateY(-6px);
        box-shadow: 0 14px 30px rgba(0,0,0,0.10);
    }

    .full {
        grid-column: 1 / -1;
    }

    /* PAYMENT */
    .highlight {
        background: linear-gradient(135deg, #ffffff, #f0f9ff);
    }

    .payment-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .amount {
        font-size: 24px;
        font-weight: bold;
        animation: popIn 0.5s ease;
    }

    /* DOCUMENTS */
    .gallery {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 14px;
    }

    .doc {
        text-align: center;
        cursor: pointer;
        transition: transform 0.2s ease;
    }

    .doc:hover {
        transform: scale(1.03);
    }

    .doc-box {
        height: 160px;
        border-radius: 12px;
        overflow: hidden;
        background: #f3f4f6;

        display: flex;
        align-items: center;
        justify-content: center;

        transition: all 0.3s ease;
    }

    .hover-zoom:hover {
        transform: scale(1.05);
    }

    .doc img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    /* MODAL */
    .modal {
        position: fixed;
        inset: 0;
        background: rgba(0,0,0,0.85);
        display: flex;
        justify-content: center;
        align-items: center;
    }

    /* modal animation */
    .modal-enter-active,
    .modal-leave-active {
        transition: all 0.25s ease;
    }

    .modal-enter-from,
    .modal-leave-to {
        opacity: 0;
        transform: scale(1.1);
    }

    .modal img {
        max-width: 90%;
        max-height: 90%;
        border-radius: 10px;
        animation: zoomIn 0.25s ease;
    }

    /* ANIMATIONS */
    .animate-up {
        animation: slideUp 0.5s ease both;
    }

    .animate-fade-down {
        animation: fadeDown 0.5s ease both;
    }

    /* BIGGER DOCUMENT BOX */
    .doc-box.large {
        height: 220px;   /* 🔥 bigger preview */
        border-radius: 14px;
        background: #f3f4f6;
        overflow: hidden;
    }

    /* PDF placeholder */
    .pdf-preview {
        font-size: 15px;
        font-weight: 600;
        color: #374151;
    }

    /* IMAGE */
    .doc-box img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    /* VIEWER (FULL SCREEN) */
    .viewer {
        position: fixed;
        inset: 0;
        background: rgba(0,0,0,0.92);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 999;
        padding: 20px;
    }

    /* IMAGE VIEW */
    .viewer-img {
        width: 85%;
        height: 85%;
        border-radius: 12px;
        box-shadow: 0 20px 60px rgba(0,0,0,0.5);
    }

    /* PDF VIEW */
    .viewer-pdf {
        width: 90%;
        height: 90%;
        border: none;
        border-radius: 12px;
        background: white;
    }

    /* fade animation */
    .fade-enter-active,
    .fade-leave-active {
        transition: opacity 0.2s ease;
    }

    .fade-enter-from,
    .fade-leave-to {
        opacity: 0;
    }

    .delay-1 { animation-delay: 0.1s; }
    .delay-2 { animation-delay: 0.2s; }
    .delay-3 { animation-delay: 0.3s; }

    @keyframes slideUp {
        from { opacity: 0; transform: translateY(20px); }
        to { opacity: 1; transform: translateY(0); }
    }

    @keyframes fadeDown {
        from { opacity: 0; transform: translateY(-15px); }
        to { opacity: 1; transform: translateY(0); }
    }

    @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
    }

    @keyframes popIn {
        from { transform: scale(0.9); opacity: 0; }
        to { transform: scale(1); opacity: 1; }
    }

    @keyframes zoomIn {
        from { transform: scale(0.9); opacity: 0; }
        to { transform: scale(1); opacity: 1; }
    }

    @keyframes pulse {
        0% { opacity: 1; }
        50% { opacity: 0.6; }
        100% { opacity: 1; }
    }

    /* RESPONSIVE */
    @media (max-width: 900px) {
        .grid {
            grid-template-columns: 1fr;
        }

        .gallery {
            grid-template-columns: 1fr;
        }
    }
</style>
