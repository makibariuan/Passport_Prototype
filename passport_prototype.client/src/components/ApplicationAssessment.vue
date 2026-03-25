<template>
  <div class="layout">

    <!-- LEFT MENU -->
    <LeftMenu @navigate="onNavigate" class="leftMenu" />

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
                <th style="text-align: center !important;" class="text-center">
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
                <td class="app-number">{{ item.number }}</td>

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
</template>

<script setup>
  import { ref, onMounted, nextTick } from "vue";
  import { useRouter } from "vue-router";
  import LeftMenu from "./LeftMenuHR.vue";
  import api from "@/api";
  import { useAuthStore } from "@/stores/auth";

  const router = useRouter();

  const searchInput = ref(null);
  const tableData = ref([]);

  let table;

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

  const fetchApplications = async () => {
    try {
      const response = await api.get(
        "/Application/GetApplicationsWithUserInfo"
      );

      tableData.value = response.data.map((item) => ({
        id: item.id,
        number: item.number,
        type: item.type,

        // ✅ format date nicely
        date: formatDate(item.date),

        // ✅ fallback if name is empty
        name: item.name || "N/A",

        status: item.status,
      }));

      await nextTick();

      if (table) {
        table.clear().rows.add(tableData.value).draw();
      }
    } catch (error) {
      console.error("Error fetching applications:", error);
    }
  };

  /* ---------------------------
     INIT TABLE
  ----------------------------*/
  onMounted(async () => {
    await fetchApplications();

    await nextTick();

    table = new window.DataTable("#assessmentTable", {
      data: tableData.value,
      columns: [
        { data: "number" },
        { data: "type" },
        { data: "date" },
        { data: "name" },
        {
          data: "status",
          render: function (data) {
            let cls = "";
            if (data.includes("Init")) cls = "init";
            else if (data.includes("Progress")) cls = "progress";
            else if (data.includes("Approved")) cls = "approved";

            return `<span class="${cls}">${data}</span>`;
          },
        },
      ],
      pageLength: 5,
      lengthChange: false,
      info: false,
    });

    // Search
    searchInput.value.addEventListener("keyup", (e) => {
      table.search(e.target.value).draw();
    });
  });

  /* ---------------------------
     NAVIGATION
  ----------------------------*/
  const onNavigate = (path) => {
    router.push(path);
  };
</script>

<style scoped>

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
    box-shadow: 4px 0 20px rgba(0,0,0,0.2);
  }

  /* ===== MAIN ===== */
  .main-container {
    margin-left: 260px;
    width: 80%;
    padding: 25px;
    overflow-y: auto;
  }

  /* ===== CARD ===== */
  .content-card {
    background: #fff;
    border-radius: 16px;
    padding: 20px 25px;
    box-shadow: 0 10px 30px rgba(0,0,0,0.05);
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
      box-shadow: 0 0 0 3px rgba(59,130,246,0.2);
    }

  /* ===== TABLE ===== */
  .table-wrapper {
    overflow-x: auto;
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
  .app-number {
    font-weight: 600;
    color: #2563eb;
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
      background: #eee;
      color: #555;
    }

  /* ===== RESPONSIVE ===== */
  @media (max-width: 768px) {
    .leftMenu {
      width: 200px;
    }

    .main-container {
      margin-left: 200px;
    }

    .search {
      width: 180px;
    }
  }
</style>
