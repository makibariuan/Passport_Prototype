<template>
  <!-- Hidden Input for File Selection (place this anywhere in your template) -->
  <input
    type="file"
    ref="requiredDocFile"
    @change="handleGenericFileSelection"
    style="display: none"
    accept=".pdf, .jpg, .jpeg, .png"
  />

  <div class="app-layout">
    <LeftMenu class="leftMenu" />

    <Header title="Personal Profile" class="header" />

    <div class="dashboard-content">
      <h2 class="page-title">Personal Profile</h2>

      <div class="important-box">
        <b class="important-header">IMPORTANT!</b>
        <p class="important-text">
          Please check all tabs (Personal, Family, Contact, Work) for required information
          <b>before</b> saving the profile.
        </p>
      </div>

      <div class="pds-top-bar">
        <div class="tab-container">
          <button
            v-for="tab in ['Personal', 'Family', 'Contact', 'Work']"
            :key="tab"
            :class="['tab-btn', { active: activeTab === tab }]"
            @click="activeTab = tab"
          >
            {{ tab }}
          </button>
        </div>
      </div>

      <!-- Folder-like Content Area -->
      <div class="tab-wrapper">
        <div class="tab-content">
          <transition name="fade-slide" mode="out-in">
            <div :key="activeTab" class="form-wrapper">
              <!-- Personal Data Sheet 1 — PERSONAL TAB -->
              <div v-if="activeTab === 'Personal'">
                <!-- ── SECTION: Identity ── -->
                <div class="pds-section">
                  <div class="pds-section-header">
                    <div>
                      <h3 class="pds-section-title">Personal Information</h3>
                    </div>
                  </div>

                  <div class="pds-field-grid">
                    <!-- Last Name -->
                    <div class="pds-field required">
                      <label class="pds-label">Last Name<span class="required-star">*</span></label>
                      <input
                        v-model="user.surname"
                        class="pds-input"
                        placeholder="e.g. Dela Cruz"
                      />
                      <span class="pds-readonly-tag">From record</span>
                    </div>

                    <!-- First Name -->
                    <div class="pds-field required">
                      <label class="pds-label"
                        >First Name<span class="required-star">*</span></label
                      >
                      <input v-model="user.firstName" class="pds-input" placeholder="e.g. Juan" />
                      <span class="pds-readonly-tag">From record</span>
                    </div>

                    <!-- Middle Name with toggle -->
                    <div class="pds-field required">
                      <label class="pds-label">
                        Middle Name
                        <span class="pds-toggle-wrap">
                          <button
                            type="button"
                            :class="['pds-yn-btn', { active: hasMiddleName === true }]"
                            @click="hasMiddleName = true"
                          >
                            Yes
                          </button>
                          <button
                            type="button"
                            :class="['pds-yn-btn', { active: hasMiddleName === false }]"
                            @click="
                              hasMiddleName = false;
                              user.middleName = '';
                            "
                          >
                            No
                          </button>
                        </span>
                      </label>
                      <input
                        v-if="hasMiddleName"
                        v-model="user.middleName"
                        class="pds-input"
                        :class="{ 'pds-input-error': showValidationErrors && !user.middleName }"
                        placeholder="e.g. Santos"
                      />
                      <div v-else class="pds-na-pill">N/A — No middle name</div>
                    </div>

                    <!-- Suffix -->
                    <div class="pds-field">
                      <label class="pds-label"
                        >Suffix <span class="pds-optional">(Optional)</span></label
                      >
                      <select v-model="user.nameExtension" class="pds-input">
                        <option value="">— None —</option>
                        <option>Jr.</option>
                        <option>Sr.</option>
                        <option>II</option>
                        <option>III</option>
                        <option>IV</option>
                        <option>V</option>
                      </select>
                    </div>

                    <!-- Date of Birth -->
                    <div class="pds-field required">
                      <label class="pds-label"
                        >Date of Birth<span class="required-star">*</span></label
                      >
                      <input v-model="user.dateOfBirth" type="date" class="pds-input" />
                      <span class="pds-readonly-tag">From record</span>
                    </div>

                    <!-- Gender Toggle -->
                    <div class="pds-field required">
                      <label class="pds-label">Sex<span class="required-star">*</span></label>
                      <div class="pds-gender-toggle">
                        <button
                          type="button"
                          :class="['pds-gender-btn male', { active: user.sex === 'male' }]"
                          @click="user.sex = 'male'"
                        >
                          <span class="gender-icon">♂</span> Male
                        </button>
                        <button
                          type="button"
                          :class="['pds-gender-btn female', { active: user.sex === 'female' }]"
                          @click="user.sex = 'female'"
                        >
                          <span class="gender-icon">♀</span> Female
                        </button>
                      </div>
                    </div>
                  </div>
                </div>

                <div class="pds-section">
                  <div class="pds-section-header">
                    <div>
                      <h3 class="pds-section-title">Civil Status & Documents</h3>
                    </div>
                  </div>

                  <div class="pds-field-grid">
                    <!-- Nationality -->
                    <div class="pds-field required">
                      <label class="pds-label"
                        >Nationality<span class="required-star">*</span></label
                      >
                      <div class="pds-flag-select">
                        <span class="pds-flag-preview">{{ selectedNationalityFlag }}</span>
                        <select
                          v-model="user.citizenship"
                          class="pds-input pds-flag-input"
                          :class="{ 'pds-input-error': showValidationErrors && !user.citizenship }"
                        >
                          <option value="">— Select Nationality —</option>
                          <option v-for="n in nationalities" :key="n.code" :value="n.name">
                            {{ n.flag }} {{ n.name }}
                          </option>
                        </select>
                      </div>
                    </div>

                    <!-- Civil Status -->
                    <div class="pds-field required">
                      <label class="pds-label"
                        >Civil Status<span class="required-star">*</span></label
                      >
                      <select
                        v-model="user.civilStatusID"
                        class="pds-input"
                        :class="{ 'pds-input-error': showValidationErrors && !user.civilStatusID }"
                      >
                        <option disabled value="">— Select Civil Status —</option>
                        <option
                          v-for="cs in civilStatuses"
                          :key="cs.civilStatusID"
                          :value="cs.civilStatusID"
                        >
                          {{ cs.statusName }}
                        </option>
                      </select>
                    </div>

                    <!-- PSA Birth Certificate Toggle -->
                    <div class="pds-field required">
                      <label class="pds-label"
                        >PSA Birth Certificate<span class="required-star">*</span></label
                      >
                      <div class="pds-yn-group">
                        <button
                          type="button"
                          :class="['pds-yn-lg', { active: hasPSABirthCert === true }]"
                          @click="hasPSABirthCert = true"
                        >
                          Yes
                        </button>
                        <button
                          type="button"
                          :class="['pds-yn-lg', 'no', { active: hasPSABirthCert === false }]"
                          @click="hasPSABirthCert = false"
                        >
                          No
                        </button>
                      </div>
                    </div>

                    <!-- Birth Legitimacy -->
                    <div class="pds-field required">
                      <label class="pds-label"
                        >Birth Legitimacy<span class="required-star">*</span></label
                      >
                      <select
                        v-model="user.birthLegitimacy"
                        class="pds-input"
                        :class="{
                          'pds-input-error': showValidationErrors && !user.birthLegitimacy,
                        }"
                      >
                        <option value="">— Select —</option>
                        <option value="Legitimate">Legitimate</option>
                        <option value="Illegitimate">Illegitimate</option>
                        <option value="Legitimated">Legitimated</option>
                      </select>
                    </div>

                    <!-- Adoptee Toggle -->
                    <div class="pds-field required">
                      <label class="pds-label">Adoptee?<span class="required-star">*</span></label>
                      <div class="pds-yn-group">
                        <button
                          type="button"
                          :class="['pds-yn-lg', { active: user.isAdoptee === true }]"
                          @click="user.isAdoptee = true"
                        >
                          Yes
                        </button>
                        <button
                          type="button"
                          :class="['pds-yn-lg', 'no', { active: user.isAdoptee === false }]"
                          @click="user.isAdoptee = false"
                        >
                          No
                        </button>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- ── SECTION: Place of Birth ── -->
                <div class="pds-section">
                  <div class="pds-section-header">
                    <div>
                      <h3 class="pds-section-title">Place of Birth</h3>
                    </div>
                  </div>

                  <div class="pds-field-grid">
                    <!-- Country of Birth -->
                    <div class="pds-field required">
                      <label class="pds-label"
                        >Country of Birth<span class="required-star">*</span></label
                      >
                      <div class="pds-flag-select">
                        <span class="pds-flag-preview">{{ selectedBirthCountryFlag }}</span>
                        <select
                          v-model="birthCountry"
                          class="pds-input pds-flag-input"
                          :class="{ 'pds-input-error': showValidationErrors && !birthCountry }"
                          @change="onBirthCountryChange"
                        >
                          <option value="">— Select Country —</option>
                          <option v-for="c in birthCountries" :key="c.code" :value="c.code">
                            {{ c.flag }} {{ c.name }}
                          </option>
                        </select>
                      </div>
                    </div>

                    <!-- Birth Region (PH only) -->
                    <div class="pds-field required" v-if="birthCountry === 'PH'">
                      <label class="pds-label">Region<span class="required-star">*</span></label>
                      <select
                        v-model="birthRegion"
                        class="pds-input"
                        :class="{ 'pds-input-error': showValidationErrors && !birthRegion }"
                        @change="onBirthRegionChange"
                      >
                        <option value="">— Select Region —</option>
                        <option v-for="r in phRegions" :key="r.code" :value="r.code">
                          {{ r.name }}
                        </option>
                      </select>
                    </div>

                    <!-- Birth Province -->
                    <div class="pds-field required" v-if="birthCountry === 'PH' && birthRegion">
                      <label class="pds-label">Province</label>
                      <select
                        v-model="birthProvince"
                        class="pds-input"
                        :class="{ 'pds-input-error': showValidationErrors && !birthProvince }"
                        @change="onBirthProvinceChange"
                      >
                        <option value="">— Select Province —</option>
                        <option v-for="p in filteredProvinces" :key="p.code" :value="p.code">
                          {{ p.name }}
                        </option>
                      </select>
                    </div>

                    <!-- Birth City/Municipality -->
                    <div class="pds-field required" v-if="birthCountry === 'PH' && birthProvince">
                      <label class="pds-label"
                        >City / Municipality<span class="required-star">*</span></label
                      >
                      <select
                        v-model="birthCity"
                        class="pds-input"
                        :class="{ 'pds-input-error': showValidationErrors && !birthCity }"
                        @change="onBirthCityChange"
                      >
                        <option value="">— Select City / Municipality —</option>
                        <option v-for="c in filteredCities" :key="c.code" :value="c.code">
                          {{ c.name }}
                        </option>
                      </select>
                    </div>

                    <!-- Birth Barangay -->
                    <div class="pds-field" v-if="birthCountry === 'PH' && birthCity">
                      <label class="pds-label">Barangay<span class="required-star">*</span></label>
                      <select v-model="birthBarangay" class="pds-input">
                        <option value="">— Select Barangay —</option>
                        <option v-for="b in filteredBarangays" :key="b.code" :value="b.code">
                          {{ b.name }}
                        </option>
                      </select>
                    </div>

                    <!-- Foreign birth: free-text city -->
                    <div class="pds-field required" v-if="birthCountry && birthCountry !== 'PH'">
                      <label class="pds-label">City / Town of Birth</label>
                      <input
                        v-model="user.placeOfBirth"
                        class="pds-input"
                        placeholder="Enter city or town"
                      />
                    </div>
                  </div>

                  <!-- Cascading breadcrumb pill -->
                  <div v-if="birthLocationSummary" class="pds-location-crumb">
                    📍 {{ birthLocationSummary }}
                  </div>
                </div>

                <!-- Save -->
                <div class="button-group-row">
                  <button @click="save" class="btn" style="margin-left: auto; width: 200px">
                    Save Progress
                  </button>
                </div>
              </div>

              <div v-else-if="activeTab === 'Family'">
                <div class="form-wrapper">
                  <!-- ── SECTION: Father Information ── -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <div>
                        <h3 class="pds-section-title">Father's Information</h3>
                      </div>
                    </div>

                    <!-- Life Status Toggle -->
                    <div class="pds-field" style="margin-bottom: 20px">
                      <label class="pds-label"
                        >Life Status<span class="required-star">*</span></label
                      >
                      <div class="pds-life-status-toggle">
                        <button
                          type="button"
                          :class="['pds-life-btn alive', { active: fatherLifeStatus === 'alive' }]"
                          @click="fatherLifeStatus = 'alive'"
                        >
                          <span>🟢</span> Alive
                        </button>
                        <button
                          type="button"
                          :class="[
                            'pds-life-btn deceased',
                            { active: fatherLifeStatus === 'deceased' },
                          ]"
                          @click="fatherLifeStatus = 'deceased'"
                        >
                          <span>⚫</span> Deceased / Absent
                        </button>
                      </div>
                    </div>

                    <div class="pds-field-grid">
                      <!-- Last Name -->
                      <div class="pds-field required">
                        <label class="pds-label"
                          >Last Name<span class="required-star">*</span></label
                        >
                        <input
                          v-model="user.fatherSurname"
                          class="pds-input"
                          :class="{
                            'pds-input-error': showValidationErrors && !user.fatherSurname,
                          }"
                          placeholder="e.g. Dela Cruz"
                        />
                      </div>

                      <!-- First Name -->
                      <div class="pds-field required">
                        <label class="pds-label"
                          >First Name<span class="required-star">*</span></label
                        >
                        <input
                          v-model="user.fatherFirstName"
                          class="pds-input"
                          :class="{
                            'pds-input-error': showValidationErrors && !user.fatherFirstName,
                          }"
                          placeholder="e.g. Jose"
                        />
                      </div>

                      <!-- Middle Name with toggle -->
                      <div class="pds-field required">
                        <label class="pds-label">
                          Middle Name
                          <span class="pds-toggle-wrap">
                            <button
                              type="button"
                              :class="['pds-yn-btn', { active: fatherHasMiddleName === true }]"
                              @click="fatherHasMiddleName = true"
                            >
                              Yes
                            </button>
                            <button
                              type="button"
                              :class="['pds-yn-btn', { active: fatherHasMiddleName === false }]"
                              @click="
                                fatherHasMiddleName = false;
                                user.fatherMiddleName = '';
                              "
                            >
                              No
                            </button>
                          </span>
                        </label>
                        <input
                          v-if="fatherHasMiddleName"
                          v-model="user.fatherMiddleName"
                          class="pds-input"
                          placeholder="e.g. Santos"
                        />
                        <div v-else class="pds-na-pill">N/A — No middle name</div>
                      </div>

                      <!-- Suffix -->
                      <div class="pds-field">
                        <label class="pds-label"
                          >Suffix <span class="pds-optional">(Optional)</span></label
                        >
                        <select v-model="user.fatherNameExtension" class="pds-input">
                          <option value="">— None —</option>
                          <option>Jr.</option>
                          <option>Sr.</option>
                          <option>II</option>
                          <option>III</option>
                          <option>IV</option>
                          <option>V</option>
                        </select>
                      </div>

                      <!-- Citizenship -->
                      <div class="pds-field required">
                        <label class="pds-label"
                          >Citizenship<span class="required-star">*</span></label
                        >
                        <div class="pds-flag-select">
                          <span class="pds-flag-preview">{{ fatherCitizenshipFlag }}</span>
                          <select
                            v-model="user.fatherCitizenship"
                            class="pds-input pds-flag-input"
                            :class="{
                              'pds-input-error': showValidationErrors && !user.fatherCitizenship,
                            }"
                          >
                            <option value="">— Select Citizenship —</option>
                            <option v-for="n in nationalities" :key="n.code" :value="n.name">
                              {{ n.flag }} {{ n.name }}
                            </option>
                          </select>
                        </div>
                      </div>
                    </div>

                    <!-- Deceased notice -->
                    <div v-if="fatherLifeStatus === 'deceased'" class="pds-status-notice deceased">
                      <span>⚫</span>
                      <span
                        >Marked as <strong>Deceased / Absent</strong>. Please still fill in the
                        information as it appears on official documents.</span
                      >
                    </div>
                  </div>

                  <!-- ── SECTION: Mother Information ── -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <div>
                        <h3 class="pds-section-title">Mother's Information</h3>
                      </div>
                    </div>

                    <!-- Life Status Toggle -->
                    <div class="pds-field" style="margin-bottom: 20px">
                      <label class="pds-label"
                        >Life Status<span class="required-star">*</span></label
                      >
                      <div class="pds-life-status-toggle">
                        <button
                          type="button"
                          :class="['pds-life-btn alive', { active: motherLifeStatus === 'alive' }]"
                          @click="motherLifeStatus = 'alive'"
                        >
                          <span>🟢</span> Alive
                        </button>
                        <button
                          type="button"
                          :class="[
                            'pds-life-btn deceased',
                            { active: motherLifeStatus === 'deceased' },
                          ]"
                          @click="motherLifeStatus = 'deceased'"
                        >
                          <span>⚫</span> Deceased / Absent
                        </button>
                      </div>
                    </div>

                    <div class="pds-field-grid">
                      <!-- Last Name (Maiden) -->
                      <div class="pds-field required">
                        <label class="pds-label">
                          Last Name
                          <span class="pds-maiden-tag">Maiden</span
                          ><span class="required-star">*</span>
                        </label>
                        <input
                          v-model="user.motherSurname"
                          class="pds-input"
                          :class="{
                            'pds-input-error': showValidationErrors && !user.motherSurname,
                          }"
                          placeholder="e.g. Reyes"
                        />
                      </div>

                      <!-- First Name -->
                      <div class="pds-field required">
                        <label class="pds-label"
                          >First Name<span class="required-star">*</span></label
                        >
                        <input
                          v-model="user.motherFirstName"
                          class="pds-input"
                          :class="{
                            'pds-input-error': showValidationErrors && !user.motherFirstName,
                          }"
                          placeholder="e.g. Maria"
                        />
                      </div>

                      <!-- Middle Name with toggle -->
                      <div class="pds-field required">
                        <label class="pds-label">
                          Middle Name
                          <span class="pds-toggle-wrap">
                            <button
                              type="button"
                              :class="['pds-yn-btn', { active: motherHasMiddleName === true }]"
                              @click="motherHasMiddleName = true"
                            >
                              Yes
                            </button>
                            <button
                              type="button"
                              :class="['pds-yn-btn', { active: motherHasMiddleName === false }]"
                              @click="
                                motherHasMiddleName = false;
                                user.motherMiddleName = '';
                              "
                            >
                              No
                            </button>
                          </span>
                        </label>
                        <input
                          v-if="motherHasMiddleName"
                          v-model="user.motherMiddleName"
                          class="pds-input"
                          placeholder="e.g. Cruz"
                        />
                        <div v-else class="pds-na-pill">N/A — No middle name</div>
                      </div>

                      <!-- Suffix -->
                      <div class="pds-field">
                        <label class="pds-label"
                          >Suffix <span class="pds-optional">(Optional)</span></label
                        >
                        <select v-model="user.motherNameExtension" class="pds-input">
                          <option value="">— None —</option>
                          <option>Jr.</option>
                          <option>Sr.</option>
                          <option>II</option>
                          <option>III</option>
                          <option>IV</option>
                          <option>V</option>
                        </select>
                      </div>

                      <!-- Citizenship -->
                      <div class="pds-field required">
                        <label class="pds-label"
                          >Citizenship<span class="required-star">*</span></label
                        >
                        <div class="pds-flag-select">
                          <span class="pds-flag-preview">{{ motherCitizenshipFlag }}</span>
                          <select
                            v-model="user.motherCitizenship"
                            class="pds-input pds-flag-input"
                            :class="{
                              'pds-input-error': showValidationErrors && !user.motherCitizenship,
                            }"
                          >
                            <option value="">— Select Citizenship —</option>
                            <option v-for="n in nationalities" :key="n.code" :value="n.name">
                              {{ n.flag }} {{ n.name }}
                            </option>
                          </select>
                        </div>
                      </div>
                    </div>

                    <!-- Deceased notice -->
                    <div v-if="motherLifeStatus === 'deceased'" class="pds-status-notice deceased">
                      <span>⚫</span>
                      <span
                        >Marked as <strong>Deceased / Absent</strong>. Please still fill in the
                        information as it appears on official documents.</span
                      >
                    </div>
                  </div>

                  <!-- Save -->
                  <div class="button-group-row">
                    <button @click="save" class="btn" style="margin-left: auto; width: 200px">
                      Save Progress
                    </button>
                  </div>
                </div>
              </div>

              <!-- Personal Data Sheet 3 -->
              <div v-else-if="activeTab === 'Contact'"></div>

              <div v-else-if="activeTab === 'Work'"></div>
            </div>
          </transition>
        </div>
      </div>
    </div>
  </div>

  <!-- Reusable dialog -->
  <DialogBox
    :show="showDialog"
    :title="dialogTitle"
    :message="dialogMessage"
    @close="handleCloseDialog"
  />

  <!-- Loading Dialog (always on top of everything) -->
  <LoadingDialog :visible="isLoading" />
</template>

<script setup>
import LeftMenu from "@/components/LeftMenu.vue";
// import Header from "./Header.vue";
import { ref, computed } from "vue";
import axios from "axios";
import DialogBox from "@/components/DialogBox.vue";
import LoadingDialog from "./LoadingDialog.vue";

const showDialog = ref(false);
const dialogTitle = ref("");
const dialogMessage = ref("");
const isLoading = ref(false);

const activeTab = ref("Personal");

// ------------------ State ------------------
const user = ref({
  surname: "",
  firstName: "",
  middleName: "",
  nameExtension: "",
  dateOfBirth: "",
  sex: "",
  citizenship: "",
  civilStatusID: "",
  birthLegitimacy: "",
  isAdoptee: false,

  // father
  fatherSurname: "",
  fatherFirstName: "",
  fatherMiddleName: "",
  fatherCitizenship: "",

  // mother
  motherSurname: "",
  motherFirstName: "",
  motherMiddleName: "",
  motherCitizenship: "",
});

const showValidationErrors = ref(false);

const onBirthCountryChange = () => {};
const onBirthRegionChange = () => {};
const onBirthProvinceChange = () => {};
const onBirthCityChange = () => {};

// ── Personal tab extras ──────────────────────────────────────────
const hasMiddleName = ref(true);
const hasPSABirthCert = ref(false);

// Extend user.value defaults
user.value.birthLegitimacy = user.value.birthLegitimacy ?? "";
user.value.isAdoptee = user.value.isAdoptee ?? false;

// Birth location cascading refs
const birthCountry = ref("PH");
const birthRegion = ref("");
const birthProvince = ref("");
const birthCity = ref("");
const birthBarangay = ref("");

// ── Nationality list (common + PH first) ────────────────────────
const nationalities = [
  { code: "PH", name: "Filipino", flag: "🇵🇭" },
  { code: "US", name: "American", flag: "🇺🇸" },
  { code: "JP", name: "Japanese", flag: "🇯🇵" },
  { code: "KR", name: "Korean", flag: "🇰🇷" },
  { code: "CN", name: "Chinese", flag: "🇨🇳" },
  { code: "AU", name: "Australian", flag: "🇦🇺" },
  { code: "GB", name: "British", flag: "🇬🇧" },
  { code: "CA", name: "Canadian", flag: "🇨🇦" },
  { code: "SG", name: "Singaporean", flag: "🇸🇬" },
  { code: "OTHER", name: "Other", flag: "🌐" },
];

const birthCountries = [
  { code: "PH", name: "Philippines", flag: "🇵🇭" },
  { code: "US", name: "United States", flag: "🇺🇸" },
  { code: "JP", name: "Japan", flag: "🇯🇵" },
  { code: "KR", name: "South Korea", flag: "🇰🇷" },
  { code: "CN", name: "China", flag: "🇨🇳" },
  { code: "AU", name: "Australia", flag: "🇦🇺" },
  { code: "GB", name: "United Kingdom", flag: "🇬🇧" },
  { code: "CA", name: "Canada", flag: "🇨🇦" },
  { code: "OTHER", name: "Other", flag: "🌐" },
];

// ── Philippine PSGC data (trimmed for brevity — extend as needed) ─
const phRegions = [
  { code: "NCR", name: "NCR — National Capital Region" },
  { code: "CAR", name: "CAR — Cordillera Administrative Region" },
  { code: "R01", name: "Region I — Ilocos Region" },
  { code: "R02", name: "Region II — Cagayan Valley" },
  { code: "R03", name: "Region III — Central Luzon" },
  { code: "R04A", name: "Region IV-A — CALABARZON" },
  { code: "MIMAROPA", name: "MIMAROPA Region" },
  { code: "R05", name: "Region V — Bicol Region" },
  { code: "R06", name: "Region VI — Western Visayas" },
  { code: "R07", name: "Region VII — Central Visayas" },
  { code: "R08", name: "Region VIII — Eastern Visayas" },
  { code: "R09", name: "Region IX — Zamboanga Peninsula" },
  { code: "R10", name: "Region X — Northern Mindanao" },
  { code: "R11", name: "Region XI — Davao Region" },
  { code: "R12", name: "Region XII — SOCCSKSARGEN" },
  { code: "R13", name: "Region XIII — Caraga" },
  { code: "BARMM", name: "BARMM — Bangsamoro" },
];

const phProvinces = [
  { code: "MM", regionCode: "NCR", name: "Metro Manila" },
  { code: "BENG", regionCode: "CAR", name: "Benguet" },
  { code: "IFUGAO", regionCode: "CAR", name: "Ifugao" },
  { code: "ILOCOS_NORTE", regionCode: "R01", name: "Ilocos Norte" },
  { code: "ILOCOS_SUR", regionCode: "R01", name: "Ilocos Sur" },
  { code: "PANGASINAN", regionCode: "R01", name: "Pangasinan" },
  { code: "BULACAN", regionCode: "R03", name: "Bulacan" },
  { code: "PAMPANGA", regionCode: "R03", name: "Pampanga" },
  { code: "CAVITE", regionCode: "R04A", name: "Cavite" },
  { code: "LAGUNA", regionCode: "R04A", name: "Laguna" },
  { code: "RIZAL", regionCode: "R04A", name: "Rizal" },
  { code: "BATANGAS", regionCode: "R04A", name: "Batangas" },
  { code: "CEBU", regionCode: "R07", name: "Cebu" },
  { code: "DAVAO_DEL_SUR", regionCode: "R11", name: "Davao del Sur" },
  // extend with full PSGC list or load from API
];

const phCities = [
  { code: "MKT", provinceCode: "MM", name: "Makati" },
  { code: "MNL", provinceCode: "MM", name: "Manila" },
  { code: "QC", provinceCode: "MM", name: "Quezon City" },
  { code: "TAGUIG", provinceCode: "MM", name: "Taguig" },
  { code: "PASIG", provinceCode: "MM", name: "Pasig" },
  { code: "PARANAQUE", provinceCode: "MM", name: "Parañaque" },
  { code: "BAGUIO", provinceCode: "BENG", name: "Baguio City" },
  { code: "MALOLOS", provinceCode: "BULACAN", name: "Malolos" },
  { code: "ANGELES", provinceCode: "PAMPANGA", name: "Angeles City" },
  { code: "DASMARIÑAS", provinceCode: "CAVITE", name: "Dasmariñas" },
  { code: "CEBU_CITY", provinceCode: "CEBU", name: "Cebu City" },
  { code: "DAVAO_CITY", provinceCode: "DAVAO_DEL_SUR", name: "Davao City" },
  // extend with full PSGC list or load from API
];

const phBarangays = [
  { code: "MKT-PO", cityCode: "MKT", name: "Poblacion" },
  { code: "MKT-BGY1", cityCode: "MKT", name: "Bel-Air" },
  { code: "MKT-BGY2", cityCode: "MKT", name: "Forbes Park" },
  { code: "MKT-BGY3", cityCode: "MKT", name: "San Lorenzo" },
  // extend per city — or load from PSGC API
];

// Cascading computed
const filteredProvinces = computed(() =>
  phProvinces.filter((p) => p.regionCode === birthRegion.value),
);
const filteredCities = computed(() =>
  phCities.filter((c) => c.provinceCode === birthProvince.value),
);
const filteredBarangays = computed(() => phBarangays.filter((b) => b.cityCode === birthCity.value));

// Flag helpers
const selectedNationalityFlag = computed(
  () => nationalities.find((n) => n.name === user.value.citizenship)?.flag ?? "🌐",
);
const selectedBirthCountryFlag = computed(
  () => birthCountries.find((c) => c.code === birthCountry.value)?.flag ?? "🌐",
);

// Location breadcrumb
const birthLocationSummary = computed(() => {
  const parts = [];
  if (birthCountry.value) {
    const c = birthCountries.find((x) => x.code === birthCountry.value);
    if (c) parts.push(c.name);
  }
  if (birthCountry.value === "PH") {
    if (birthRegion.value)
      parts.push(phRegions.find((r) => r.code === birthRegion.value)?.name ?? "");
    if (birthProvince.value)
      parts.push(phProvinces.find((p) => p.code === birthProvince.value)?.name ?? "");
    if (birthCity.value) parts.push(phCities.find((c) => c.code === birthCity.value)?.name ?? "");
    if (birthBarangay.value)
      parts.push(phBarangays.find((b) => b.code === birthBarangay.value)?.name ?? "");
  }
  return parts.filter(Boolean).join(" › ");
});

const updatePersonal = async () => {
  try {
    isLoading.value = true;

    const payload = {
      surname: user.value.surname,
      firstName: user.value.firstName,
      middleName: hasMiddleName.value ? user.value.middleName : null,
      nameExtension: user.value.nameExtension,
      dateOfBirth: user.value.dateOfBirth,
      sex: user.value.sex,
      citizenship: user.value.citizenship,
      civilStatusID: user.value.civilStatusID,
      birthLegitimacy: user.value.birthLegitimacy,
      isAdoptee: user.value.isAdoptee,
      hasPSABirthCert: hasPSABirthCert.value,

      // birth location
      birthCountry: birthCountry.value,
      birthRegion: birthRegion.value,
      birthProvince: birthProvince.value,
      birthCity: birthCity.value,
      birthBarangay: birthBarangay.value,
      placeOfBirth: birthCountry.value !== "PH" ? user.value.placeOfBirth : null,
    };

    await axios.patch("/user/personal", payload);

    dialogTitle.value = "Success";
    dialogMessage.value = "Personal info saved.";
    showDialog.value = true;
  } catch (err) {
    console.log("error: ", err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to save personal info.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
  }
};

// API FETCH

const updateFamily = async () => {
  try {
    isLoading.value = true;

    const payload = {
      father: {
        surname: user.value.fatherSurname,
        firstName: user.value.fatherFirstName,
        middleName: fatherHasMiddleName.value ? user.value.fatherMiddleName : null,
        nameExtension: user.value.fatherNameExtension,
        citizenship: user.value.fatherCitizenship,
        lifeStatus: fatherLifeStatus.value,
      },

      mother: {
        surname: user.value.motherSurname,
        firstName: user.value.motherFirstName,
        middleName: motherHasMiddleName.value ? user.value.motherMiddleName : null,
        nameExtension: user.value.motherNameExtension,
        citizenship: user.value.motherCitizenship,
        lifeStatus: motherLifeStatus.value,
      },
    };

    await axios.patch("/user/family", payload);

    dialogTitle.value = "Success";
    dialogMessage.value = "Family info saved.";
    showDialog.value = true;
  } catch (err) {
    console.log("error: ", err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to save family info.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
  }
};

const save = () => {
  showValidationErrors.value = true;

  if (activeTab.value === "Personal") {
    updatePersonal();
  } else if (activeTab.value === "Family") {
    updateFamily();
  }
};

// ── Family tab extras ────────────────────────────

const fatherLifeStatus = ref("alive"); // 'alive' | 'deceased'
const motherLifeStatus = ref("alive");

const fatherHasMiddleName = ref(true);
const motherHasMiddleName = ref(true);

// Extend user.value defaults for new fields
user.value.fatherCitizenship = user.value.fatherCitizenship ?? "";
user.value.motherCitizenship = user.value.motherCitizenship ?? "";

// Flag computed helpers for parent citizenship selects
const fatherCitizenshipFlag = computed(
  () => nationalities.find((n) => n.name === user.value.fatherCitizenship)?.flag ?? "🌐",
);
const motherCitizenshipFlag = computed(
  () => nationalities.find((n) => n.name === user.value.motherCitizenship)?.flag ?? "🌐",
);

// ------------------ Save Methods ------------------
</script>

<style>
/* ************************************************************************** */
/* 1. DASHBOARD GRID LAYOUT STYLES (MATCHING EmployeeID.vue) */
/* ************************************************************************** */
/* Renamed .app-grid-layout to .app-layout and updated style */
.app-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  /* Change this to ensure the header doesn't push the content down */
  grid-template-rows: auto 1fr;
  height: 100vh;
  background-color: #f4f7f9;
  overflow: hidden; /* Prevent double scrollbars on the body */
}

.leftMenu {
  grid-column: 1;
  grid-row: 1 / span 2;
  z-index: 100;
}

.header {
  grid-column: 2;
  grid-row: 1;
  z-index: 90;
}

/* Renamed .content-area to .dashboard-content and updated style */
.dashboard-content {
  grid-column: 2;
  grid-row: 2;
  width: 100%;
  height: 100%; /* Fill the remaining grid space */
  padding: 30px;
  box-sizing: border-box;
  overflow-y: auto; /* This allows the dashboard to scroll if needed */
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* ************************************************************************** */
/* 2. PAGE CONTENT & TABS STYLES (MATCHING EmployeeID.vue) */
/* ************************************************************************** */

.page-title {
  font-size: 1.8rem;
  color: #06195e;
  font-weight: 800;
  margin-bottom: 25px;
}

/* Sub-title style updated to match EmployeeID.vue */
.sub-title {
  font-size: 1.5rem;
  color: #06195e;
  padding-bottom: 10px;
  border-bottom: 2px solid #f0f4f8;
  font-weight: 700;
}

/* Adopted the boxed tab style from EmployeeID.vue */
.tab-container {
  display: flex;
  gap: 2px;
  margin-bottom: 0;
  z-index: 2; /* Keep tabs above the content wrapper border */
}

.tab-btn {
  padding: 12px 24px;
  border: none;
  background: #e2e8f0;
  color: #4a5568;
  font-weight: 600;
  border-radius: 10px 10px 0 0;
  cursor: pointer;
  transition: all 0.2s ease;
}

.tab-btn:last-child {
  border-right: none;
}

.tab-btn:hover:not(.active) {
  background: #e0e0e0;
  color: #004085;
}

.tab-btn.active {
  background: white;
  color: #06195e;
  border-top: 3px solid #06195e;
  padding-top: 9px; /* Adjust for border */
}

/* Updated .tab-wrapper to look like .data-section-card */
.tab-wrapper {
  background: white;
  border-radius: 0 12px 12px 12px;
  padding: 25px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  /* Dynamic height fix */
  flex: 1; /* Automatically takes up all remaining space below the tabs */
  overflow-y: auto; /* This is where the scrollbar should live */
  display: flex;
  flex-direction: column;
}

/* Added data-section-card for consistency */
.data-section-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
  padding: 20px;
  overflow: hidden;
}

.tab-content {
  /* No specific styles needed here */
}

.form-wrapper {
  padding: 10px;
}

/* Tailwind-like classes (retained) */
.w-\[90\%\] {
  width: 90%;
}

.mx-auto {
  margin-left: auto;
  margin-right: auto;
}

/* ************************************************************************** */
/* 3. PDS Table Layout (UPDATED COLORS) */
/* ************************************************************************** */
.pds-table-wrapper {
  width: 100%;
  overflow-x: auto; /* Allows horizontal scrolling if columns are too wide */
  margin-bottom: 20px;
  /*    border: 1px solid #ccc;*/
  border-radius: 8px;
}

.pds-table thead th {
  background-color: #f8f9fa;
  color: #333;
  font-weight: 600;
  text-align: center;
  white-space: nowrap;
}

.pds-table {
  width: 100%;
  min-width: 1100px; /* Ensures the table doesn't collapse too much */
  border-collapse: collapse;
  font-size: 0.85rem;
}

.pds-table th,
.pds-table td {
  padding: 8px 5px;
  /*      border: 1px solid #e0e0e0;*/
  vertical-align: middle;
}

/* Only targets tables with the 'table-compact' class */
.table-compact td:last-child {
  width: 1%;
  white-space: nowrap;
  text-align: center;
  padding: 5px 15px;
}

/* Ensures the input (first column) takes the rest of the space */
.table-compact td:first-child {
  width: auto;
}

/* Ensures the input inside this specific table stays full width */
.table-compact .auth-input {
  width: 100%;
}

/* Add this to override the min-width for Other Info tables */
.pds-table.table-compact {
  min-width: 0 !important;
  /* This allows the table to be smaller than 1100px */
}

/* Optional: Give the first column a fixed-ish width
    so the delete buttons align vertically across all three Other Info tables */
.table-compact td:first-child {
  width: 80%; /* Adjust this percentage to control where the delete button starts */
}

/* Specific Column Widths */
.col-level {
  width: 120px;
}

.col-school {
  width: 220px;
}

.col-degree {
  width: 200px;
}

.col-year {
  width: 80px;
}
/* Narrower for YYYY inputs */
.col-units {
  width: 150px;
}

.col-honors {
  width: 150px;
}

.col-action {
  width: 80px;
}

.pds-table .auth-input {
  width: 100%;
  padding: 6px;
  margin: 0;
  box-sizing: border-box;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
}

.text-lg {
  font-size: 1rem;
  line-height: 1.6;
  color: #1a202c;
}

.spacer-row {
  background: transparent !important;
}

.flex.items-center.space-x-6 {
  display: flex;
  gap: 24px;
  align-items: center;
}

.flex.justify-between.items-start.gap-8 {
  display: flex;
  gap: 32px;
  margin-top: 20px;
}

.flex-grow {
  flex: 1;
}

input[type="radio"],
input[type="checkbox"] {
  accent-color: #06195e; /* Colors the radio/checkbox Makati Blue */
  width: 18px;
  height: 18px;
  cursor: pointer;
}

/* This makes the rows slide to their new positions smoothly */
.list-move {
  transition: transform 0.4s cubic-bezier(0.55, 0, 0.1, 1);
}

/* Optional: add a slight fade when rows move */
.list-enter-active,
.list-leave-active {
  transition: all 0.4s ease;
}

.pds-table-wrapper {
  overflow: hidden; /* Prevents scrollbar flickering during animation */
}

/* ══════════════════════════════════════════════════
    PDS REDESIGN — Section Cards & New Controls
  ══════════════════════════════════════════════════ */

.pds-section {
  background: #fff;
  border: 1px solid #e8edf4;
  border-radius: 14px;
  padding: 24px 28px;
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(6, 25, 94, 0.05);
  transition: box-shadow 0.2s;
}
.pds-section:hover {
  box-shadow: 0 4px 16px rgba(6, 25, 94, 0.09);
}

.pds-section-header {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 20px;
  padding-bottom: 14px;
  border-bottom: 2px solid #f0f4f9;
}
.pds-section-icon {
  font-size: 1.5rem;
  width: 44px;
  height: 44px;
  background: #eef2fb;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.pds-section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #06195e;
  margin: 0 0 2px;
}
.pds-section-subtitle {
  font-size: 0.78rem;
  color: #8796aa;
  margin: 0;
}

/* Field grid */
.pds-field-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 18px 20px;
}
.pds-field {
  display: flex;
  flex-direction: column;
  gap: 5px;
  position: relative;
}
.pds-field.pds-field-wide {
  grid-column: span 3;
}
.pds-field.pds-field-narrow {
  grid-column: span 1;
  min-width: 100px;
}
.pds-field.pds-field-half {
  grid-column: span 2;
}

.pds-label {
  font-size: 0.75rem;
  font-weight: 700;
  color: #4a5568;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.pds-optional {
  font-weight: 400;
  text-transform: none;
  color: #a0aec0;
  font-size: 0.72rem;
}

.pds-input {
  width: 100%;
  padding: 9px 12px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  font-size: 0.9rem;
  color: #1a202c;
  background: #fff;
  box-sizing: border-box;
  transition:
    border-color 0.18s,
    box-shadow 0.18s;
}
.pds-input:focus {
  outline: none;
  border-color: #06195e;
  box-shadow: 0 0 0 3px rgba(6, 25, 94, 0.1);
}
.pds-input:disabled {
  background: #f1f5f9;
  color: #718096;
  cursor: not-allowed;
}
.pds-input-error {
  border-color: #e53e3e !important;
  background: #fff5f5 !important;
}

.pds-readonly-tag {
  font-size: 0.68rem;
  color: #a0aec0;
  font-style: italic;
  position: absolute;
  bottom: -16px;
  right: 2px;
}

/* Gender toggle */
.pds-gender-toggle {
  display: flex;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  overflow: hidden;
}
.pds-gender-btn {
  flex: 1;
  padding: 9px;
  border: none;
  background: #f8fafc;
  color: #718096;
  font-weight: 600;
  font-size: 0.88rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  transition: all 0.18s;
}
.pds-gender-btn:first-child {
  border-right: 1px solid #d1d9e6;
}
.pds-gender-btn.male.active {
  background: #dbeafe;
  color: #1d4ed8;
}
.pds-gender-btn.female.active {
  background: #fce7f3;
  color: #be185d;
}
.gender-icon {
  font-size: 1.1rem;
}

/* Yes/No inline toggle (for labels) */
.pds-toggle-wrap {
  display: inline-flex;
  border: 1px solid #d1d9e6;
  border-radius: 6px;
  overflow: hidden;
  margin-left: auto;
}
.pds-yn-btn {
  padding: 2px 10px;
  font-size: 0.7rem;
  font-weight: 700;
  border: none;
  background: #f8fafc;
  color: #718096;
  cursor: pointer;
  transition: all 0.15s;
}
.pds-yn-btn.active {
  background: #06195e;
  color: #fff;
}

/* Yes/No large group */
.pds-yn-group {
  display: flex;
  gap: 8px;
}
.pds-yn-lg {
  flex: 1;
  padding: 9px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  background: #f8fafc;
  color: #718096;
  font-weight: 700;
  font-size: 0.82rem;
  cursor: pointer;
  transition: all 0.18s;
}
.pds-yn-lg.active {
  background: #ebf8f0;
  color: #276749;
  border-color: #68d391;
}
.pds-yn-lg.no.active {
  background: #fff5f5;
  color: #c53030;
  border-color: #fc8181;
}

/* N/A pill */
.pds-na-pill {
  padding: 9px 12px;
  background: #f1f5f9;
  border: 1.5px dashed #cbd5e0;
  border-radius: 8px;
  font-size: 0.82rem;
  color: #a0aec0;
  font-style: italic;
}

/* Flag select */
.pds-flag-select {
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  padding: 0 10px;
  background: #fff;
  transition:
    border-color 0.18s,
    box-shadow 0.18s;
}
.pds-flag-select:focus-within {
  border-color: #06195e;
  box-shadow: 0 0 0 3px rgba(6, 25, 94, 0.1);
}
.pds-flag-preview {
  font-size: 1.4rem;
  flex-shrink: 0;
}
.pds-flag-input {
  border: none !important;
  box-shadow: none !important;
  padding-left: 0 !important;
  flex: 1;
  background: transparent;
}

/* Upload slot */
.pds-upload-slot {
  margin-top: 8px;
}
.pds-upload-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  background: #f0f4ff;
  color: #06195e;
  border: 1.5px dashed #93a8d4;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.82rem;
  cursor: pointer;
  transition: all 0.18s;
}
.pds-upload-btn:hover {
  background: #e0e9ff;
  border-color: #06195e;
}

.pds-file-chip {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: #f0fff4;
  border: 1px solid #68d391;
  border-radius: 20px;
  padding: 5px 12px;
  font-size: 0.82rem;
  max-width: 100%;
}
.pds-file-name {
  color: #276749;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 200px;
}
.pds-file-name:hover {
  text-decoration: underline;
}
.pds-file-del {
  background: none;
  border: none;
  color: #e53e3e;
  font-size: 1rem;
  cursor: pointer;
  padding: 0;
  line-height: 1;
  flex-shrink: 0;
}

/* Marriage doc notice */
.pds-marriage-doc-row {
  margin-top: 16px;
  padding: 14px 18px;
  background: #fffbeb;
  border-left: 4px solid #f59e0b;
  border-radius: 0 8px 8px 0;
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}
.pds-doc-notice {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  flex: 1;
  font-size: 0.83rem;
  color: #78350f;
  line-height: 1.5;
}
.pds-doc-notice-icon {
  font-size: 1.1rem;
  flex-shrink: 0;
}

/* Location breadcrumb */
.pds-location-crumb {
  margin-top: 14px;
  padding: 10px 16px;
  background: #f0f7ff;
  border: 1px solid #bfdbfe;
  border-radius: 8px;
  font-size: 0.82rem;
  color: #1d4ed8;
  font-weight: 600;
}

/* Address layout */
.pds-address-row {
  display: grid;
  grid-template-columns: 1fr 120px;
  gap: 12px;
  margin-bottom: 14px;
}

/* ── Life Status Toggle ─────────────────────────────── */
.pds-life-status-toggle {
  display: flex;
  gap: 10px;
}

.pds-life-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border-radius: 10px;
  border: 1.5px solid #d1d9e6;
  background: #f8fafc;
  color: #718096;
  font-weight: 700;
  font-size: 0.85rem;
  cursor: pointer;
  transition: all 0.18s;
}

.pds-life-btn.alive.active {
  background: #f0fff4;
  color: #276749;
  border-color: #68d391;
}

.pds-life-btn.deceased.active {
  background: #f7fafc;
  color: #4a5568;
  border-color: #a0aec0;
}

/* ── Deceased / Status Notice ───────────────────────── */
.pds-status-notice {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 16px;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 0.83rem;
  line-height: 1.5;
}

.pds-status-notice.deceased {
  background: #f7fafc;
  border: 1px solid #cbd5e0;
  color: #4a5568;
}

/* ── Maiden name tag ─────────────────────────────────── */
.pds-maiden-tag {
  font-size: 0.65rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  background: #e9d8fd;
  color: #6b46c1;
  padding: 2px 7px;
  border-radius: 10px;
  margin-left: 4px;
}

/* Gov IDs grid */
.pds-ids-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 14px;
  margin-top: 18px;
  padding-top: 16px;
  border-top: 1px solid #f0f4f9;
}
.pds-id-field {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

/* Subsection label */
.pds-subsection-label {
  font-size: 0.78rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  color: #06195e;
  margin: 0 0 12px;
  padding-bottom: 6px;
  border-bottom: 1px dashed #d1d9e6;
}

/* Children list */
.pds-children-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 10px;
}
.pds-child-row {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #f8fafc;
  border-radius: 8px;
  padding: 8px 10px;
}
.pds-child-num {
  width: 24px;
  height: 24px;
  background: #06195e;
  color: white;
  border-radius: 50%;
  font-size: 0.72rem;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.pds-empty-list {
  text-align: center;
  padding: 16px;
  color: #a0aec0;
  font-style: italic;
  font-size: 0.85rem;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px dashed #d1d9e6;
}
.pds-remove-btn {
  background: none;
  border: 1px solid #fc8181;
  color: #e53e3e;
  border-radius: 6px;
  padding: 4px 8px;
  cursor: pointer;
  font-size: 0.82rem;
  transition: all 0.15s;
}
.pds-remove-btn:hover {
  background: #e53e3e;
  color: white;
}

.pds-add-row-btn {
  padding: 9px 18px;
  background: white;
  border: 1.5px dashed #06195e;
  color: #06195e;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.82rem;
  cursor: pointer;
  transition: all 0.18s;
}
.pds-add-row-btn:hover {
  background: #eef2fb;
}

.important-box {
  background: #fff8e1;
  border: 1px solid #facc15;
  border-left: 5px solid #f59e0b;
  border-radius: 10px;
  padding: 14px 16px;
  margin-bottom: 20px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

.important-header {
  font-weight: 800;
  color: #92400e;
  font-size: 0.9rem;
  margin-bottom: 4px;
  display: flex;
  align-items: center;
  gap: 6px;
}

.important-text {
  font-size: 0.85rem;
  color: #78350f;
  margin: 0;
  line-height: 1.5;
}

/* ************************************************************************** */
/* 5. Buttons (UPDATED TO PRIMARY/SECONDARY AESTHETIC) */
/* ************************************************************************** */
.btn {
  /* Primary Button (Next) */
  background-color: #007bff;
  color: white;
  padding: 10px 15px;
  border-radius: 6px;
  font-weight: 600; /* Slightly less bold */
  cursor: pointer;
  border: 1px solid #007bff;
  transition: all 0.2s;
}

.btn:hover {
  background-color: #0056b3;
  border-color: #0056b3;
}

.btn-white {
  padding: 10px 20px;
  background: white;
  border: 1px solid #06195e;
  color: #06195e;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-white:hover {
  background: #f0f4f8;
  transform: translateY(-1px);
}

/* ************************************************************************** */
/* 7. Transition Styles (Retained/Updated) */
/* ************************************************************************** */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition:
    opacity 0.3s ease,
    transform 0.3s ease;
}

.fade-slide-enter-from {
  opacity: 0;
  transform: translateY(10px); /* Changed from X to Y to match EmployeeID */
}

.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-10px); /* Changed from X to Y to match EmployeeID */
}

.required-star {
  color: #e53935; /* nice red */
  font-weight: 800;
}

/* ************************************************************************** */
/* 8. RESPONSIVENESS (MATCHING EmployeeID.vue & MOBILE SWAP) */
/* ************************************************************************** */
@media (max-width: 1200px) {
  .app-layout {
    grid-template-columns: 20vw 1fr;
  }
}

@media (max-width: 992px) {
  .app-layout {
    grid-template-columns: 80px 1fr;
  }
  /* Tab buttons wrap on smaller screens */
  .tab-container {
    flex-wrap: wrap;
    border: none;
    box-shadow: none;
    background: none;
  }

  .tab-btn {
    flex: 1 1 auto;
    border: 1px solid #e0e0e0;
    border-radius: 6px;
    margin: 5px;
    border-right: 1px solid #e0e0e0; /* Override internal border reset */
  }
}

@media (max-width: 768px) {
  .app-layout {
    grid-template-columns: 1fr;
    /* Define 3 rows: Header, Content, Menu */
    grid-template-rows: auto auto 1fr;
  }

  .leftMenu,
  .header,
  .dashboard-content {
    grid-column: 1;
  }

  .header {
    grid-row: 1; /* Header stays on top */
  }

  /* PDS Content moved to row 2 (just below the header) */
  .dashboard-content {
    grid-row: 2;
    padding: 20px 15px;
  }

  /* Left Menu moved to row 3 (below the PDS Content) */
  .leftMenu {
    grid-row: 3;
    width: 100%;
    min-height: auto;
  }
}
</style>
