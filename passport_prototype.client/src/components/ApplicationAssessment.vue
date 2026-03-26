<template>
  <div class="layout">
    <!-- LEFT MENU -->
    <!--<LeftMenu @navigate="onNavigate" class="leftMenu" />-->
    <!-- MAIN CONTENT -->
    <div class="main-container">
      <div class="content-card">
        <!-- HEADER -->
        <div class="header">
          <div>
            <h2>📄 Application Assessment</h2>
            <p class="sub">Manage and review applications</p>
          </div>
        </div>

        <!-- TABLE -->
        <div class="table-wrapper">
          <table id="assessmentTable" class="custom-table">
            <thead>
              <tr>
                <th style="text-align: center !important" class="text-center">
                  Application Number#
                </th>
                <th>Application Type</th>
                <th>Date</th>
                <th>Person Name</th>
                <th>Status</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="item in tableData" :key="item.id">

                <td class="app-number">
                  <span class="clickable-code">{{ item.number }}</span>
                </td>
                <td>
                  <span class="badge type">{{ item.type }}</span>
                </td>

                <td>{{ item.date }}</td>

                <td>{{ item.name }}</td>

                <td>
                  <span :class="['badge', statusClass(item.status)]">
                    {{ item.status }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>

  <DialogBox :show="showDetails" title="Application Details" @close="showDetails = false">
    <div v-if="selectedApp" class="visitor-details-pass">
      <div class="pass-body">
        <div class="pass-visual">
          <label class="pass-label">DOCUMENT PREVIEW</label>
          <img :src="getFullImageUrl(selectedApp.validIdPath)"
               class="pass-id-image"
               alt="Valid ID"
               @error="(e) => e.target.src = 'https://via.placeholder.com/150'" />

          <div class="pass-status-tag" :class="statusClass(selectedApp.status).replace('status-', '')">
            {{ getStatusLabel(selectedApp.status).toUpperCase() }}
          </div>
        </div>

        <div class="pass-info-grid">
          <div class="info-item full-width">
            <label>Full Name</label>
            <p>{{ selectedApp.name }}</p>
          </div>

          <div class="info-item">
            <label>Access Code</label>
            <p class="code-text">{{ selectedApp.number }}</p>
          </div>

          <div class="info-item">
            <label>Application Type</label>
            <p><span class="badge type">{{ selectedApp.type }}</span></p>
          </div>

          <div class="info-item">
            <label>Schedule</label>
            <p>{{ selectedApp.date }}</p>
          </div>

          <div class="info-item">
            <label>Citizen Type</label>
            <p>{{ selectedApp.raw.citizenType || 'N/A' }}</p>
          </div>

          <div class="info-item full-width">
            <label>Country / Site</label>
            <p>{{ selectedApp.raw.country || 'N/A' }} — {{ selectedApp.raw.site || 'N/A' }}</p>
          </div>

          <div class="info-item">
            <label>Courtesy Lane</label>
            <p>{{ selectedApp.isCourtesyLane ? 'YES' : 'NO' }}</p>
          </div>

          <div class="info-item">
            <label>Payment Status</label>
            <p :class="selectedApp.isPaid ? 'status-approved' : 'status-rejected'">
              {{ selectedApp.isPaid ? 'PAID' : 'UNPAID' }}
            </p>
          </div>
        </div>
      </div>

      <div class="pass-footer">
        <button @click="showDetails = false" class="close-details-btn">Close Details</button>
      </div>
    </div>
  </DialogBox>


</template>

<script setup>
import { ref, onMounted, nextTick } from "vue";
import { useRouter } from "vue-router";
import LeftMenu from "./LeftMenuHR.vue";
  import axios from "axios";

  import DialogBox from "@/components/DialogBox.vue";


const router = useRouter();

const tableData = ref([]);

  let table;

  const showDetails = ref(false);
  const selectedApp = ref(null);

  const openDetails = (item) => {
    // Find the original raw data from tableData if you need fields not in the table
    selectedApp.value = item;
    showDetails.value = true;
  };
/* ---------------------------
     FETCH DATA FROM API
  ----------------------------*/

const formatDate = (dateString) => {
  if (!dateString) return "";

  const date = new Date(dateString);

  return date.toLocaleString("en-US", {
    month: "2-digit",
    day: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

  const statusClass = (status) => {
    // Integers representing success/approval
    if ([4, 5, 6].includes(status)) return "status-approved";
    // Integers representing pending/init
    if ([0, 7].includes(status)) return "status-pending";
    // Integers representing rejections
    if (status === 99) return "status-rejected";
    // Everything else is "in progress" blue
    return "status-pending";
  };

  const getStatusLabel = (status) => {
    const statusMap = {
      0: "Pending Schedule",
      1: "Scheduled",
      2: "Captured",
      3: "Adjudication",
      4: "Validated",
      5: "Printed / Exported",
      6: "Active / Card Issued",
      7: "For Approval",
      99: "Rejected / Fraud",
      100: "Display Only"
    };
    return statusMap[status] || "Unknown";
  };

  const fetchApplications = async () => {
    try {
      const response = await axios.get(
        "https://localhost:5000/api/Application/GetApplicationsWithUserInfo"
      );

      // Log the entire response data to the console
      console.log("API Response:", response.data);

      tableData.value = response.data.map((item) => {
        const fullName = [item.firstName, item.middleName, item.lastName]
          .filter(Boolean)
          .join(" ") || "N/A";

        return {
          id: item.enrollmentId,
          number: item.enrollmentAccessCode,
          type: item.appType,
          date: formatDate(item.schedule),
          name: fullName,
          status: item.applicationStatus,
          isCourtesyLane: item.isCourtesyLane,
          isPaid: item.isPaid,
          // Add the image paths here
          validIdPath: item.validIdPath,
          certificatePath: item.certificatePath,
          raw: item
        };
      });

      await nextTick();

      // If using DataTables
      if (table) {
        table.clear().rows.add(tableData.value).draw();
      }
    } catch (error) {
      console.error("Error fetching applications:", error);
    }
  };


  const getFullImageUrl = (path) => {
    if (!path) return 'https://via.placeholder.com/150';
    let cleanPath = path.replace(/\\/g, '/');
    // Strip wwwroot if it exists in the path string
    cleanPath = cleanPath.replace(/^wwwroot\//, '').replace(/^\/wwwroot\//, '');

    const API_BASE_URL = 'https://localhost:5000'; // Match your Application API port
    return `${API_BASE_URL}${cleanPath.startsWith('/') ? '' : '/'}${cleanPath}`;
  };

/* ---------------------------
     INIT TABLE
  ----------------------------*/
onMounted(async () => {
  await fetchApplications();

  await nextTick();

  // Replace the columns array inside your onMounted:
  table = new window.DataTable("#assessmentTable", {
    data: tableData.value,
    columns: [
      {
        data: "number",
        className: "app-number",
        render: function (data) {
          // Restoring the clickable logic for the first column
          return `<span class="clickable-code view-details-trigger">${data}</span>`;
        }
      },
      { data: "type" },
      { data: "date" },
      { data: "name" },
      {
        data: "status",
        render: function (data) {
          const label = getStatusLabel(data);
          const cls = statusClass(data).replace('status-', '');
          // Note: statusClass already handles the logic for approved/pending/rejected

          // Fallback logic for class names to match your badge CSS
          let badgeCls = 'default';
          if (cls === 'approved') badgeCls = 'approved';
          if (cls === 'pending') badgeCls = 'init';
          if (cls === 'rejected') badgeCls = 'rejected';

          return `<span class="badge ${badgeCls}">${label}</span>`;
        },
      },
    ],
    pageLength: 5,
    lengthChange: false,
    info: false,
    // Helpful addition to ensure it fits your layout
    responsive: true,
    autoWidth: false
  });

  const tableElement = document.querySelector('#assessmentTable');

  tableElement.addEventListener('click', (e) => {
    // Check if the clicked element is our trigger span
    if (e.target.classList.contains('view-details-trigger')) {
      // Get the row data from DataTables
      const rowData = table.row(e.target.closest('tr')).data();
      if (rowData) {
        openDetails(rowData);
      }
    }
  });

  //// Search
  //searchInput.value.addEventListener("keyup", (e) => {
  //  table.search(e.target.value).draw();
  //});
});

/* ---------------------------
     NAVIGATION
  ----------------------------*/
const onNavigate = (path) => {
  router.push(path);
};
</script>

<style scoped>
.status-approved {
  color: #28a745;
  font-weight: bold;
}

.status-pending {
  color: #ffc107;
  font-weight: bold;
}

.status-rejected {
  color: #dc3545;
  font-weight: bold;
}

.text-center {
  text-align: center;
}

.table-wrapper {
  width: 100%;
  overflow-x: auto; /* enables scroll ONLY if needed */
}

/* ===== LAYOUT ===== */
.layout {
  display: flex;
  height: 100vh;
  background: #eef2f7;
}

/* ===== SIDEBAR ===== */
.leftMenu {
  position: fixed;
  width: 260px;
  height: 100vh;
  background: linear-gradient(180deg, #0b1f75, #06195e);
  color: white;
  box-shadow: 4px 0 20px rgba(0, 0, 0, 0.2);
}

/* ===== MAIN ===== */
  .main-container {
    /* margin: 0 auto;  <- Remove or keep, but the below is more robust */
    display: flex;
    justify-content: center;
    align-items: flex-start;
    width: 100%;
    padding: 25px;
    /* Add this to prevent padding from pushing the width past 100% */
    box-sizing: border-box;
    /* Prevent the horizontal scroll */
    overflow-x: hidden;
  }

/* ===== CARD ===== */
  .content-card {
    background: #fff;
    border-radius: 16px;
    padding: 20px 25px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
    /* Use max-width instead of width: 100% to ensure it doesn't overflow */
    width: 100%;
    max-width: 1200px;
    box-sizing: border-box;
  }

/* ===== HEADER ===== */
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.header h2 {
  margin: 0;
  font-weight: 600;
}

.sub {
  margin-top: 5px;
  font-size: 13px;
  color: #888;
}

/* ===== SEARCH ===== */
.search {
  padding: 10px 14px;
  width: 250px;
  border-radius: 10px;
  border: 1px solid #ddd;
  outline: none;
  transition: 0.2s;
}

.search:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2);
}

/* ===== TABLE ===== */
  .table-wrapper {
    width: 100%;
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

/* FORCE table to fit container */
.custom-table {
  width: 100% !important;
  table-layout: auto;
  border-collapse: collapse;
}
/* Prevent text from stretching columns */
.custom-table th,
.custom-table td {
  white-space: nowrap;
}

/* Optional: prevent super long text breaking layout */
.custom-table td {
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* HEADER */
.custom-table thead {
  background: #f9fafb;
}

.custom-table th {
  padding: 12px;
  text-align: left;
  font-size: 13px;
  color: #666;
}

/* HOVER */
.custom-table tr:hover {
  background: #f1f5ff;
}

/* APPLICATION NUMBER */
  :deep(td.app-number) {
    padding: 0 !important;
    cursor: pointer !important;
  }

/* ===== BADGES ===== */
.badge {
  padding: 5px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}

/* TYPE */
.badge.type {
  background: #e0f2fe;
  color: #0284c7;
}

/* STATUS */
.badge.init {
  background: #fef3c7;
  color: #d97706;
}

.badge.progress {
  background: #dbeafe;
  color: #2563eb;
}

.badge.approved {
  background: #dcfce7;
  color: #16a34a;
}

  .badge.default {
    background: #f3f4f6;
    color: #374151;
  }

  :deep(.clickable-code) {
    cursor: pointer !important;
    display: inline-block;
    width: 100%;
    color: #2563eb !important;
    text-decoration: underline;
    font-weight: 700;
    position: relative;
    z-index: 10; /* Ensure it stays above table cell layers */
  }

  :deep(.view-details-trigger) {
    cursor: pointer !important;
  }

  .app-details-grid {
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 10px 0;
  }

  .detail-item {
    font-size: 0.95rem;
    color: #374151;
  }

    .detail-item strong {
      color: #111827;
      width: 140px;
      display: inline-block;
    }

  hr {
    border: 0;
    border-top: 1px solid #e5e7eb;
    margin: 10px 0;
  }

  /* VMS Inspired Pass Layout */
  .visitor-details-pass {
    padding: 5px;
    max-width: 650px;
  }

  .pass-body {
    display: flex;
    gap: 25px;
    margin-bottom: 20px;
  }

  .pass-visual {
    flex: 0 0 200px;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .pass-label {
    font-size: 0.7rem;
    font-weight: 800;
    color: #64748b;
    letter-spacing: 1px;
  }

  .pass-id-image {
    width: 100%;
    aspect-ratio: 4/3;
    object-fit: cover;
    border-radius: 12px;
    border: 1px solid #e2e8f0;
    box-shadow: 0 4px 12px rgba(0,0,0,0.08);
  }

  .pass-info-grid {
    flex: 1;
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 15px;
  }

  .info-item label {
    display: block;
    font-size: 0.75rem;
    color: #94a3b8;
    font-weight: 600;
    margin-bottom: 2px;
    text-transform: uppercase;
  }

  .info-item p {
    font-weight: 600;
    color: #1e293b;
    margin: 0;
    font-size: 0.95rem;
  }

  .full-width {
    grid-column: span 2;
  }

  .pass-status-tag {
    text-align: center;
    padding: 8px;
    border-radius: 6px;
    font-size: 0.75rem;
    font-weight: 800;
  }

    /* Match your existing status logic to VMS colors */
    .pass-status-tag.pending {
      background: #fef3c7;
      color: #92400e;
    }

    .pass-status-tag.approved {
      background: #dcfce7;
      color: #166534;
    }

    .pass-status-tag.rejected {
      background: #fee2e2;
      color: #991b1b;
    }

  .close-details-btn {
    width: 100%;
    padding: 12px;
    background: #3b82f6; /* Matching your primary blue */
    border: none;
    border-radius: 8px;
    font-weight: 700;
    color: white;
    cursor: pointer;
    transition: background 0.2s;
  }

    .close-details-btn:hover {
      background: #2563eb;
    }

  .code-text {
    font-family: 'Courier New', Courier, monospace;
    color: #003366 !important;
  }

  .badge.rejected {
    background: #fee2e2;
    color: #991b1b;
  }

  /* Also ensure the Dialog status tag handles the class correctly */
  .pass-status-tag.rejected {
    background: #fee2e2;
    color: #991b1b;
  }

  /* Add to your <style scoped> */
  :deep(.clickable-code) {
    cursor: pointer !important;
    display: inline-block;
    width: 100%;
    color: #2563eb !important;
    text-decoration: underline;
    font-weight: 700;
    position: relative;
    z-index: 10; /* Ensure it stays above table cell layers */
  }

  :deep(.view-details-trigger) {
    cursor: pointer !important;
  }

  .app-details-grid {
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 10px 0;
  }

  .detail-item {
    font-size: 0.95rem;
    color: #374151;
  }

    .detail-item strong {
      color: #111827;
      width: 140px;
      display: inline-block;
    }

  hr {
    border: 0;
    border-top: 1px solid #e5e7eb;
    margin: 10px 0;
  }

  /* VMS Inspired Pass Layout */
  .visitor-details-pass {
    padding: 5px;
    max-width: 650px;
  }

  .pass-body {
    display: flex;
    gap: 25px;
    margin-bottom: 20px;
  }

  .pass-visual {
    flex: 0 0 200px;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .pass-label {
    font-size: 0.7rem;
    font-weight: 800;
    color: #64748b;
    letter-spacing: 1px;
  }

  .pass-id-image {
    width: 100%;
    aspect-ratio: 4/3;
    object-fit: cover;
    border-radius: 12px;
    border: 1px solid #e2e8f0;
    box-shadow: 0 4px 12px rgba(0,0,0,0.08);
  }

  .pass-info-grid {
    flex: 1;
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 15px;
  }

  .info-item label {
    display: block;
    font-size: 0.75rem;
    color: #94a3b8;
    font-weight: 600;
    margin-bottom: 2px;
    text-transform: uppercase;
  }

  .info-item p {
    font-weight: 600;
    color: #1e293b;
    margin: 0;
    font-size: 0.95rem;
  }

  .full-width {
    grid-column: span 2;
  }

  .pass-status-tag {
    text-align: center;
    padding: 8px;
    border-radius: 6px;
    font-size: 0.75rem;
    font-weight: 800;
  }

    /* Match your existing status logic to VMS colors */
    .pass-status-tag.pending {
      background: #fef3c7;
      color: #92400e;
    }

    .pass-status-tag.approved {
      background: #dcfce7;
      color: #166534;
    }

    .pass-status-tag.rejected {
      background: #fee2e2;
      color: #991b1b;
    }

  .close-details-btn {
    width: 100%;
    padding: 12px;
    background: #3b82f6; /* Matching your primary blue */
    border: none;
    border-radius: 8px;
    font-weight: 700;
    color: white;
    cursor: pointer;
    transition: background 0.2s;
  }

    .close-details-btn:hover {
      background: #2563eb;
    }

  .code-text {
    font-family: 'Courier New', Courier, monospace;
    color: #003366 !important;
  }

/* ===== RESPONSIVE ===== */


  @media (max-width: 768px) {
    .leftMenu {
      width: 200px;
    }

    .main-container {
      margin-left: 0; /* Ensure no margin on small screens */
      padding: 15px;
    }

    .content-card {
      padding: 15px;
    }

    .search {
      width: 180px;
    }
  }
</style>
