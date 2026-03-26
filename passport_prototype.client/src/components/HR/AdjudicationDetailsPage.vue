<template>
  <div class="app-layout">
    <Header title="Adjudication Resolution" />

    <div class="dashboard-content">
      <div class="header-flex">
        <button @click="$router.back()" class="back-link">← Back to List</button>
        <h2 class="page-title">Conflict Adjudication</h2>
      </div>

      <div class="comparison-grid">
        <div class="comparison-card current">
          <div class="card-tag tag-new">Current Applicant</div>
          <div class="photo-placeholder">
            <img :src="applicant.photo || '/default-user.png'" alt="Applicant">
          </div>
          <div class="info-list">
            <div class="info-item"><label>Full Name:</label> <span>{{ applicant.fullName }}</span></div>
            <div class="info-item"><label>Birth Date:</label> <span>{{ applicant.birthDate }}</span></div>
            <div class="info-item"><label>Department:</label> <span>{{ applicant.department }}</span></div>
            <div class="info-item"><label>Application Date:</label> <span>{{ applicant.dateCapture }}</span></div>
          </div>
        </div>

        <div class="vs-divider"></div>

        <div class="comparison-card hit">
          <div class="card-tag tag-hit">Potential Duplicate (Hit)</div>
          <div class="photo-placeholder">
            <img :src="hitRecord.photo || '/default-user.png'" alt="Match">
          </div>
          <div class="info-list">
            <div class="info-item"><label>Full Name:</label> <span>{{ hitRecord.fullName }}</span></div>
            <div class="info-item"><label>Birth Date:</label> <span>{{ hitRecord.birthDate }}</span></div>
            <div class="info-item"><label>Employee ID:</label> <span>{{ hitRecord.employeeID }}</span></div>
            <div class="info-item"><label>Status:</label> <span>Active</span></div>
          </div>
        </div>
      </div>

      <div class="decision-footer">
        <div class="reason-box">
          <label>Adjudication Remarks:</label>
          <textarea v-model="remarks" placeholder="Explain why this hit is being resolved/rejected..."></textarea>
        </div>
        <div class="button-group">
          <button @click="resolveHit" class="btn btn-success">✅ Confirm as Unique (No Hit)</button>
          <button @click="rejectHit" class="btn btn-danger">❌ Confirm Duplicate (Reject)</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api from '@/api';

const route = useRoute();
const router = useRouter();

// --- State ---
const isLoading = ref(false);
const remarks = ref("");
const applicant = ref({
  fullName: "Loading...",
  birthDate: "",
  department: "",
  dateCapture: "",
  photo: null,
  afisPersonHit: null
});

const hitRecord = ref({
  fullName: "Loading...",
  birthDate: "",
  employeeID: "",
  photo: null
});

// --- Fetch Data ---
const fetchData = async () => {
  const personId = route.params.id;
  try {
    isLoading.value = true;

    // 1. Fetch Current Applicant Details
    // Adjust endpoint based on your API (e.g., /api/employee/{id})
    const applicantRes = await api.get(`/employee/details/${personId}`);
    applicant.value = applicantRes.data;

    // 2. Fetch the Hit Record (The match)
    // The applicant data should contain the AFISPersonHit ID
    if (applicant.value.afisPersonHit) {
      const hitRes = await api.get(`/employee/details/${applicant.value.afisPersonHit}`);
      hitRecord.value = hitRes.data;
    }
  } catch (error) {
    console.error("Error fetching adjudication data:", error);
    alert("Failed to load applicant details.");
  } finally {
    isLoading.value = false;
  }
};

// --- Action: Resolve Hit (Confirm Unique) ---
const resolveHit = async () => {
  if (!remarks.value) {
    alert("Please provide adjudication remarks before resolving.");
    return;
  }

  if (!confirm("Are you sure this person is UNIQUE? This will move them to the Printing queue.")) return;

  try {
    isLoading.value = true;
    // Calling the API to set status to 4 (Validated) and clear the Hit flag
    await api.post(`/afis/resolve-hit`, {
      personId: applicant.value.id,
      remarks: remarks.value
    });

    alert("Adjudication successful. Record marked as Unique.");
    router.push({ name: 'ManageHRApplications', params: { statusType: 'adjudication' } });
  } catch (error) {
    console.error("Resolution failed:", error);
    alert("Failed to resolve adjudication.");
  } finally {
    isLoading.value = false;
  }
};

// --- Action: Reject Hit (Confirm Duplicate) ---
const rejectHit = async () => {
  if (!remarks.value) {
    alert("Please provide adjudication remarks before rejecting.");
    return;
  }

  if (!confirm("Confirming this as a DUPLICATE will cancel this application. Proceed?")) return;

  try {
    isLoading.value = true;
    // Calling API to mark as a confirmed duplicate/rejected
    await api.post(`/afis/reject-hit`, {
      personId: applicant.value.id,
      remarks: remarks.value
    });

    alert("Record confirmed as duplicate and application rejected.");
    router.push({ name: 'ManageHRApplications', params: { statusType: 'adjudication' } });
  } catch (error) {
    console.error("Rejection failed:", error);
    alert("Failed to process rejection.");
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchData();
});
</script>

<style scoped>
  .app-layout {
    width: 100%;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    background-color: #f8fafc; /* Optional: light gray background */
  }

  .dashboard-content {
    width: 100%;
    max-width: 1200px;
    /* This margin: 0 auto only works if the parent allows it */
    margin: 0 auto;
    padding: 40px 20px;
    box-sizing: border-box;
    /* Ensures that if the content is short, it still stays centered */
    display: flex;
    flex-direction: column;
  }

  .dashboard-content {
    max-width: 1200px; /* Limits the width for better readability */
    margin: 0 auto; /* This does the actual centering */
    padding: 40px 20px; /* Gives some breathing room on top and sides */
    width: 100%;
    box-sizing: border-box;
  }

  .header-flex {
    display: flex;
    align-items: center;
    gap: 20px;
    margin-bottom: 30px;
  }

  .page-title {
    margin: 0;
    font-size: 1.8rem;
    color: #1e293b;
  }

  .comparison-grid {
    display: grid;
    grid-template-columns: 1fr 100px 1fr;
    gap: 20px;
    align-items: center;
    margin-top: 20px;
  }

  .comparison-card {
    background: white;
    border-radius: 12px;
    padding: 30px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.1);
    position: relative;
    border-top: 8px solid #ced4da;
  }

    .comparison-card.current {
      border-top-color: #007bff;
    }

    .comparison-card.hit {
      border-top-color: #dc3545;
    }

  .card-tag {
    position: absolute;
    top: -15px;
    left: 20px;
    padding: 4px 15px;
    border-radius: 20px;
    color: white;
    font-weight: bold;
    font-size: 0.8em;
  }

  .tag-new {
    background: #007bff;
  }

  .tag-hit {
    background: #dc3545;
  }

  .photo-placeholder {
    width: 200px;
    height: 200px;
    background: #eee;
    margin: 0 auto 20px;
    border-radius: 8px;
    overflow: hidden;
  }

    .photo-placeholder img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

  .info-item {
    margin-bottom: 10px;
    border-bottom: 1px solid #f0f0f0;
    padding-bottom: 5px;
  }

    .info-item label {
      font-weight: bold;
      color: #555;
      width: 130px;
      display: inline-block;
    }

  .vs-divider {
    text-align: center;
    font-weight: 900;
    font-size: 2em;
    color: #ccc;
  }

  .decision-footer {
    margin-top: 40px;
    background: #fff;
    padding: 25px;
    border-radius: 12px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.05);
  }

  .reason-box textarea {
    width: 100%;
    height: 80px;
    margin-top: 10px;
    border-radius: 8px;
    border: 1px solid #ddd;
    padding: 10px;
  }

  .button-group {
    display: flex;
    justify-content: center;
    gap: 20px;
    margin-top: 20px;
  }

  .btn {
    padding: 12px 30px;
    border-radius: 8px;
    font-weight: bold;
    cursor: pointer;
    border: none;
  }

  .btn-success {
    background: #28a745;
    color: white;
  }

  .btn-danger {
    background: #dc3545;
    color: white;
  }
</style>
