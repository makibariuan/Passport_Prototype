<template>
  <!-- Hidden Input for File Selection -->
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

      <div class="tab-wrapper">
        <div class="tab-content">
          <transition name="fade-slide" mode="out-in">
            <div :key="activeTab" class="form-wrapper">
              <!-- ═══════════════════════════════════════════ -->
              <!-- PERSONAL TAB                               -->
              <!-- ═══════════════════════════════════════════ -->
              <div v-if="activeTab === 'Personal'">
                <!-- SECTION: Personal Information -->
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
                        v-model="user.lastName"
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
                      <input v-model="user.birthdate" type="date" class="pds-input" />
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
                <!-- /SECTION: Personal Information -->
                <!-- SECTION: Civil Status & Documents -->
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
                      <select v-model="user.civilStatusID" class="pds-input">
                        <option disabled value="">— Select Civil Status —</option>
                        <option>Single</option>
                        <option>Married</option>
                        <option>Widowed</option>
                        <option>Legally Separated</option>
                      </select>
                    </div>

                    <!-- PSA Birth Certificate -->
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

                    <!-- Adoptee -->
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
                <!-- /SECTION: Civil Status & Documents -->
                <!-- SECTION: Place of Birth -->
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
                      <label class="pds-label">Province<span class="required-star">*</span></label>
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

                  <!-- Location breadcrumb -->
                  <div v-if="birthLocationSummary" class="pds-location-crumb">
                    📍 {{ birthLocationSummary }}
                  </div>
                </div>
                <!-- /SECTION: Place of Birth -->
                <!-- Save -->
                <div class="button-group-row">
                  <button
                    @click="save"
                    class="btn save-btn"
                    style="margin-left: auto; width: 200px"
                  >
                    Save Progress
                  </button>
                </div>
              </div>
              <!-- /PERSONAL TAB -->
              <!-- ═══════════════════════════════════════════ -->
              <!-- FAMILY TAB                                 -->
              <!-- ═══════════════════════════════════════════ -->
              <div v-else-if="activeTab === 'Family'">
                <div class="form-wrapper">
                  <!-- SECTION: Father's Information -->
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

                      <!-- Middle Name -->
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
                  <!-- /SECTION: Father -->
                  <!-- SECTION: Mother's Information -->
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

                      <!-- Middle Name -->
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
                  <!-- /SECTION: Mother -->
                  <!-- Save -->
                  <div class="button-group-row">
                    <button
                      @click="save"
                      class="btn save-btn"
                      style="margin-left: auto; width: 200px"
                    >
                      Save Progress
                    </button>
                  </div>
                </div>
              </div>
              <!-- /FAMILY TAB -->
              <!-- ═══════════════════════════════════════════ -->

              <!-- CONTACT TAB                                -->
              <!-- ═══════════════════════════════════════════ -->
              <div v-else-if="activeTab === 'Contact'">
                <div class="form-wrapper">
                  <!-- SECTION: Current Address -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <div class="section-icon-wrap contact">
                        <svg
                          width="18"
                          height="18"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                          stroke-width="2"
                        >
                          <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                          />
                          <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                          />
                        </svg>
                      </div>
                      <div>
                        <h3 class="pds-section-title">Current Address</h3>
                        <p class="pds-section-sub">Your residential address details</p>
                      </div>
                    </div>

                    <div class="pds-field-grid">
                      <!-- Street / Unit / Building -->
                      <div class="pds-field contact-full">
                        <label class="pds-label"
                          >Street / Unit / Building<span class="required-star">*</span></label
                        >
                        <input
                          v-model="address.street"
                          class="pds-input"
                          placeholder="e.g. 123 Rizal St., Unit 4B"
                        />
                      </div>

                      <!-- Address Abroad -->
                      <div class="pds-field contact-full">
                        <label class="pds-label"
                          >Address Abroad <span class="pds-optional">(if applicable)</span></label
                        >
                        <input
                          v-model="address.abroad"
                          class="pds-input"
                          placeholder="Foreign address if currently abroad"
                        />
                      </div>

                      <!-- Country -->
                      <div class="pds-field">
                        <label class="pds-label">Country<span class="required-star">*</span></label>
                        <div class="pds-flag-select">
                          <span class="pds-flag-preview">{{ selectedAddressCountryFlag }}</span>
                          <select
                            v-model="address.country"
                            class="pds-input pds-flag-input"
                            @change="onAddressCountryChange"
                          >
                            <option value="">— Select Country —</option>
                            <option v-for="c in birthCountries" :key="c.code" :value="c.code">
                              {{ c.flag }} {{ c.name }}
                            </option>
                          </select>
                        </div>
                      </div>

                      <!-- Region (PH only) -->
                      <div class="pds-field" v-if="address.country === 'PH'">
                        <label class="pds-label">Region<span class="required-star">*</span></label>
                        <select
                          v-model="address.region"
                          class="pds-input"
                          @change="onAddressRegionChange"
                        >
                          <option value="">— Select Region —</option>
                          <option v-for="r in phRegions" :key="r.code" :value="r.code">
                            {{ r.name }}
                          </option>
                        </select>
                      </div>

                      <!-- Province (PH only) -->
                      <div class="pds-field" v-if="address.country === 'PH' && address.region">
                        <label class="pds-label"
                          >Province<span class="required-star">*</span></label
                        >
                        <select
                          v-model="address.province"
                          class="pds-input"
                          @change="onAddressProvinceChange"
                        >
                          <option value="">— Select Province —</option>
                          <option
                            v-for="p in filteredAddressProvinces"
                            :key="p.code"
                            :value="p.code"
                          >
                            {{ p.name }}
                          </option>
                        </select>
                      </div>

                      <!-- Municipality (PH only) -->
                      <div class="pds-field" v-if="address.country === 'PH' && address.province">
                        <label class="pds-label"
                          >City / Municipality<span class="required-star">*</span></label
                        >
                        <select
                          v-model="address.municipality"
                          class="pds-input"
                          @change="onAddressMunicipalityChange"
                        >
                          <option value="">— Select City / Municipality —</option>
                          <option v-for="c in filteredAddressCities" :key="c.code" :value="c.code">
                            {{ c.name }}
                          </option>
                        </select>
                      </div>

                      <!-- Barangay (PH only) -->
                      <div
                        class="pds-field"
                        v-if="address.country === 'PH' && address.municipality"
                      >
                        <label class="pds-label">Barangay</label>
                        <select v-model="address.barangay" class="pds-input">
                          <option value="">— Select Barangay —</option>
                          <option
                            v-for="b in filteredAddressBarangays"
                            :key="b.code"
                            :value="b.code"
                          >
                            {{ b.name }}
                          </option>
                        </select>
                      </div>

                      <!-- Foreign city (non-PH) -->
                      <div class="pds-field" v-if="address.country && address.country !== 'PH'">
                        <label class="pds-label">City / Town</label>
                        <input
                          v-model="address.city"
                          class="pds-input"
                          placeholder="Enter city or town"
                        />
                      </div>

                      <!-- Postal Code -->
                      <div class="pds-field">
                        <label class="pds-label">Postal Code</label>
                        <input
                          v-model="address.postal"
                          class="pds-input"
                          placeholder="e.g. 1200"
                          maxlength="10"
                        />
                      </div>
                    </div>

                    <!-- Address breadcrumb -->
                    <div v-if="addressLocationSummary" class="pds-location-crumb">
                      📍 {{ addressLocationSummary }}
                    </div>
                  </div>
                  <!-- /SECTION: Current Address -->
                  <!-- SECTION: Phone Numbers -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <div class="section-icon-wrap phone">
                        <svg
                          width="18"
                          height="18"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                          stroke-width="2"
                        >
                          <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 8V5z"
                          />
                        </svg>
                      </div>
                      <div>
                        <h3 class="pds-section-title">Contact Numbers</h3>
                        <p class="pds-section-sub">Personal mobile and landline</p>
                      </div>
                    </div>

                    <div class="contact-phone-grid">
                      <!-- Personal Mobile -->
                      <div class="phone-card">
                        <div class="phone-card-label">
                          <span class="phone-type-badge mobile">📱 Mobile</span>
                        </div>
                        <div class="phone-input-row">
                          <select class="phone-country-select" v-model="contact.mobileCountry">
                            <option value="+63">🇵🇭 +63</option>
                            <option value="+1">🇺🇸 +1</option>
                            <option value="+44">🇬🇧 +44</option>
                            <option value="+61">🇦🇺 +61</option>
                            <option value="+81">🇯🇵 +81</option>
                            <option value="+82">🇰🇷 +82</option>
                            <option value="+65">🇸🇬 +65</option>
                          </select>
                          <input
                            class="pds-input phone-segment"
                            placeholder="906"
                            v-model="contact.mobilePrefix"
                            maxlength="4"
                          />
                          <span class="phone-dash">—</span>
                          <input
                            class="pds-input phone-main"
                            placeholder="1234567"
                            v-model="contact.mobileNumber"
                            maxlength="8"
                          />
                        </div>
                        <div
                          class="phone-preview"
                          v-if="contact.mobilePrefix || contact.mobileNumber"
                        >
                          {{ contact.mobileCountry }} {{ contact.mobilePrefix
                          }}{{ contact.mobileNumber }}
                        </div>
                      </div>

                      <!-- Personal Landline -->
                      <div class="phone-card">
                        <div class="phone-card-label">
                          <span class="phone-type-badge landline">☎️ Landline</span>
                        </div>
                        <div class="phone-input-row">
                          <select class="phone-country-select" v-model="contact.landlineCountry">
                            <option value="+63">🇵🇭 +63</option>
                            <option value="+1">🇺🇸 +1</option>
                            <option value="+44">🇬🇧 +44</option>
                            <option value="+61">🇦🇺 +61</option>
                            <option value="+81">🇯🇵 +81</option>
                          </select>
                          <input
                            class="pds-input phone-main"
                            placeholder="02-8123-4567"
                            v-model="contact.landlineNumber"
                          />
                        </div>
                        <div class="phone-preview" v-if="contact.landlineNumber">
                          {{ contact.landlineCountry }} {{ contact.landlineNumber }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <!-- /SECTION: Phone Numbers -->
                  <!-- Save -->
                  <div class="button-group-row">
                    <button
                      @click="save"
                      class="btn save-btn"
                      style="margin-left: auto; width: 200px"
                    >
                      <svg
                        width="16"
                        height="16"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                        stroke-width="2.5"
                      >
                        <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                      </svg>
                      Save Progress
                    </button>
                  </div>
                </div>
              </div>
              <!-- /CONTACT TAB -->
              <!-- ═══════════════════════════════════════════ -->

              <!-- WORK TAB                                   -->
              <!-- ═══════════════════════════════════════════ -->
              <div v-else-if="activeTab === 'Work'">
                <div class="form-wrapper">
                  <!-- SECTION: Job Details -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <div class="section-icon-wrap work">
                        <svg
                          width="18"
                          height="18"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                          stroke-width="2"
                        >
                          <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"
                          />
                        </svg>
                      </div>
                      <div>
                        <h3 class="pds-section-title">Job Details</h3>
                        <p class="pds-section-sub">Your current occupation and employer</p>
                      </div>
                    </div>

                    <div class="pds-field-grid">
                      <div class="pds-field contact-full">
                        <label class="pds-label">Occupation / Job Title</label>
                        <input
                          v-model="work.occupation"
                          class="pds-input"
                          placeholder="e.g. Software Engineer, Teacher, Nurse"
                        />
                      </div>

                      <div class="pds-field contact-full">
                        <label class="pds-label">Employer / Company Name</label>
                        <input
                          v-model="work.employer"
                          class="pds-input"
                          placeholder="e.g. Acme Corporation"
                        />
                      </div>

                      <div class="pds-field contact-full">
                        <label class="pds-label">Office Address</label>
                        <input
                          v-model="work.officeAddress"
                          class="pds-input"
                          placeholder="e.g. 10th Floor, BGC Tower, Taguig"
                        />
                      </div>
                    </div>
                  </div>
                  <!-- /SECTION: Job Details -->
                  <!-- SECTION: Office Location -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <div class="section-icon-wrap office-loc">
                        <svg
                          width="18"
                          height="18"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                          stroke-width="2"
                        >
                          <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-2 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
                          />
                        </svg>
                      </div>
                      <div>
                        <h3 class="pds-section-title">Office Location</h3>
                        <p class="pds-section-sub">Where your office is located</p>
                      </div>
                    </div>

                    <div class="pds-field-grid">
                      <!-- Country -->
                      <div class="pds-field">
                        <label class="pds-label">Country</label>
                        <div class="pds-flag-select">
                          <span class="pds-flag-preview">{{ selectedOfficeCountryFlag }}</span>
                          <select
                            v-model="work.officeCountry"
                            class="pds-input pds-flag-input"
                            @change="onOfficeCountryChange"
                          >
                            <option value="">— Select Country —</option>
                            <option v-for="c in birthCountries" :key="c.code" :value="c.code">
                              {{ c.flag }} {{ c.name }}
                            </option>
                          </select>
                        </div>
                      </div>

                      <!-- Region (PH only) -->
                      <div class="pds-field" v-if="work.officeCountry === 'PH'">
                        <label class="pds-label">Region</label>
                        <select
                          v-model="work.officeRegion"
                          class="pds-input"
                          @change="onOfficeRegionChange"
                        >
                          <option value="">— Select Region —</option>
                          <option v-for="r in phRegions" :key="r.code" :value="r.code">
                            {{ r.name }}
                          </option>
                        </select>
                      </div>

                      <!-- Province (PH only) -->
                      <div
                        class="pds-field"
                        v-if="work.officeCountry === 'PH' && work.officeRegion"
                      >
                        <label class="pds-label">Province</label>
                        <select
                          v-model="work.officeProvince"
                          class="pds-input"
                          @change="onOfficeProvinceChange"
                        >
                          <option value="">— Select Province —</option>
                          <option
                            v-for="p in filteredOfficeProvinces"
                            :key="p.code"
                            :value="p.code"
                          >
                            {{ p.name }}
                          </option>
                        </select>
                      </div>

                      <!-- Municipality (PH only) -->
                      <div
                        class="pds-field"
                        v-if="work.officeCountry === 'PH' && work.officeProvince"
                      >
                        <label class="pds-label">City / Municipality</label>
                        <select
                          v-model="work.officeMunicipality"
                          class="pds-input"
                          @change="onOfficeMunicipalityChange"
                        >
                          <option value="">— Select City / Municipality —</option>
                          <option v-for="c in filteredOfficeCities" :key="c.code" :value="c.code">
                            {{ c.name }}
                          </option>
                        </select>
                      </div>

                      <!-- Barangay (PH only) -->
                      <div
                        class="pds-field"
                        v-if="work.officeCountry === 'PH' && work.officeMunicipality"
                      >
                        <label class="pds-label">Barangay</label>
                        <select v-model="work.officeBarangay" class="pds-input">
                          <option value="">— Select Barangay —</option>
                          <option
                            v-for="b in filteredOfficeBarangays"
                            :key="b.code"
                            :value="b.code"
                          >
                            {{ b.name }}
                          </option>
                        </select>
                      </div>

                      <!-- Foreign city (non-PH) -->
                      <div
                        class="pds-field"
                        v-if="work.officeCountry && work.officeCountry !== 'PH'"
                      >
                        <label class="pds-label">City / Town</label>
                        <input
                          v-model="work.officeCity"
                          class="pds-input"
                          placeholder="Enter city or town"
                        />
                      </div>

                      <!-- Postal Code -->
                      <div class="pds-field">
                        <label class="pds-label">Postal Code</label>
                        <input
                          v-model="work.postalCode"
                          class="pds-input"
                          placeholder="e.g. 1634"
                          maxlength="10"
                        />
                      </div>
                    </div>

                    <!-- Office breadcrumb -->
                    <div v-if="officeLocationSummary" class="pds-location-crumb">
                      🏢 {{ officeLocationSummary }}
                    </div>
                  </div>
                  <!-- /SECTION: Office Location -->
                  <!-- SECTION: Work Contact Numbers -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <div class="section-icon-wrap phone">
                        <svg
                          width="18"
                          height="18"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                          stroke-width="2"
                        >
                          <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 8V5z"
                          />
                        </svg>
                      </div>
                      <div>
                        <h3 class="pds-section-title">Work Contact Numbers</h3>
                        <p class="pds-section-sub">Work mobile and office landline</p>
                      </div>
                    </div>

                    <div class="contact-phone-grid">
                      <!-- Work Mobile -->
                      <div class="phone-card">
                        <div class="phone-card-label">
                          <span class="phone-type-badge mobile">📱 Work Mobile</span>
                        </div>
                        <div class="phone-input-row">
                          <select class="phone-country-select" v-model="work.workMobileCountry">
                            <option value="+63">🇵🇭 +63</option>
                            <option value="+1">🇺🇸 +1</option>
                            <option value="+44">🇬🇧 +44</option>
                            <option value="+61">🇦🇺 +61</option>
                            <option value="+81">🇯🇵 +81</option>
                            <option value="+82">🇰🇷 +82</option>
                            <option value="+65">🇸🇬 +65</option>
                          </select>
                          <input
                            class="pds-input phone-segment"
                            placeholder="906"
                            v-model="work.workMobilePrefix"
                            maxlength="4"
                          />
                          <span class="phone-dash">—</span>
                          <input
                            class="pds-input phone-main"
                            placeholder="1234567"
                            v-model="work.workMobileNumber"
                            maxlength="8"
                          />
                        </div>
                        <div
                          class="phone-preview"
                          v-if="work.workMobilePrefix || work.workMobileNumber"
                        >
                          {{ work.workMobileCountry }} {{ work.workMobilePrefix
                          }}{{ work.workMobileNumber }}
                        </div>
                      </div>

                      <!-- Work Landline -->
                      <div class="phone-card">
                        <div class="phone-card-label">
                          <span class="phone-type-badge landline">☎️ Office Landline</span>
                        </div>
                        <div class="phone-input-row">
                          <select class="phone-country-select" v-model="work.workLandlineCountry">
                            <option value="+63">🇵🇭 +63</option>
                            <option value="+1">🇺🇸 +1</option>
                            <option value="+44">🇬🇧 +44</option>
                            <option value="+61">🇦🇺 +61</option>
                          </select>
                          <input
                            class="pds-input phone-main"
                            placeholder="02-8123-4567"
                            v-model="work.workLandlineNumber"
                          />
                        </div>
                        <div class="phone-preview" v-if="work.workLandlineNumber">
                          {{ work.workLandlineCountry }} {{ work.workLandlineNumber }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <!-- /SECTION: Work Contact Numbers -->
                  <!-- Save -->
                  <div class="button-group-row">
                    <button
                      @click="save"
                      class="btn save-btn"
                      style="margin-left: auto; width: 200px"
                    >
                      <svg
                        width="16"
                        height="16"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                        stroke-width="2.5"
                      >
                        <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                      </svg>
                      Save Progress
                    </button>
                  </div>
                </div>
              </div>
              <!-- /WORK TAB -->
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

  <!-- Loading Dialog -->
  <LoadingDialog :visible="isLoading" />
</template>

<script setup>
import LeftMenu from "@/components/LeftMenu.vue";
import { ref, computed, onMounted } from "vue";
import axios from "axios";
import DialogBox from "@/components/DialogBox.vue";
import LoadingDialog from "./LoadingDialog.vue";
import { useAuthStore } from "../stores/auth";

// ─────────────────────────────────────────────
// GLOBAL STATE
// ─────────────────────────────────────────────
const showDialog = ref(false);
const dialogTitle = ref("");
const dialogMessage = ref("");
const isLoading = ref(false);
const activeTab = ref("Personal");
const userId = 5;
const Auth = useAuthStore();
const showValidationErrors = ref(false);

// ─────────────────────────────────────────────
// USER / PERSONAL
// ─────────────────────────────────────────────
const user = ref({
  // Personal
  passportPersonalInformationId: 0,
  lastName: "",
  firstName: "",
  middleName: "",
  nameExtension: "",
  birthdate: "",
  sex: "",
  citizenship: "",
  civilStatusID: "",
  birthLegitimacy: "",
  isAdoptee: false,
  placeOfBirth: "",
  // Family — father
  fatherSurname: "",
  fatherFirstName: "",
  fatherMiddleName: "",
  fatherNameExtension: "",
  fatherCitizenship: "",
  // Family — mother
  motherSurname: "",
  motherFirstName: "",
  motherMiddleName: "",
  motherNameExtension: "",
  motherCitizenship: "",
});

// Personal tab extras
const hasMiddleName = ref(true);
const hasPSABirthCert = ref(false);

// ─────────────────────────────────────────────
// CONTACT STATE
// ─────────────────────────────────────────────
const contact = ref({
  mobileCountry: "+63",
  mobilePrefix: "",
  mobileNumber: "",
  landlineCountry: "+63",
  landlineNumber: "",
});

const address = ref({
  street: "",
  abroad: "",
  country: "",
  region: "",
  province: "",
  municipality: "",
  barangay: "",
  city: "", // for non-PH free-text
  postal: "",
});

// ─────────────────────────────────────────────
// WORK STATE
// ─────────────────────────────────────────────
const work = ref({
  occupation: "",
  employer: "",
  officeAddress: "",
  officeCountry: "",
  officeRegion: "",
  officeProvince: "",
  officeMunicipality: "",
  officeBarangay: "",
  officeCity: "", // for non-PH free-text
  postalCode: "",
  workMobileCountry: "+63",
  workMobilePrefix: "",
  workMobileNumber: "",
  workLandlineCountry: "+63",
  workLandlineNumber: "",
});

// ─────────────────────────────────────────────
// FAMILY EXTRAS
// ─────────────────────────────────────────────
const fatherLifeStatus = ref("alive");
const motherLifeStatus = ref("alive");
const fatherHasMiddleName = ref(true);
const motherHasMiddleName = ref(true);

// ─────────────────────────────────────────────
// REFERENCE DATA
// ─────────────────────────────────────────────
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
];

const phBarangays = [
  { code: "MKT-PO", cityCode: "MKT", name: "Poblacion" },
  { code: "MKT-BGY1", cityCode: "MKT", name: "Bel-Air" },
  { code: "MKT-BGY2", cityCode: "MKT", name: "Forbes Park" },
  { code: "MKT-BGY3", cityCode: "MKT", name: "San Lorenzo" },
];

// ─────────────────────────────────────────────
// BIRTH LOCATION STATE & HANDLERS
// ─────────────────────────────────────────────
const birthCountry = ref("PH");
const birthRegion = ref("");
const birthProvince = ref("");
const birthCity = ref("");
const birthBarangay = ref("");

const onBirthCountryChange = () => {
  birthRegion.value = "";
  birthProvince.value = "";
  birthCity.value = "";
  birthBarangay.value = "";
};
const onBirthRegionChange = () => {
  birthProvince.value = "";
  birthCity.value = "";
  birthBarangay.value = "";
};
const onBirthProvinceChange = () => {
  birthCity.value = "";
  birthBarangay.value = "";
};
const onBirthCityChange = () => {
  birthBarangay.value = "";
};

// ─────────────────────────────────────────────
// ADDRESS CASCADING HANDLERS
// ─────────────────────────────────────────────
const onAddressCountryChange = () => {
  address.value.region = "";
  address.value.province = "";
  address.value.municipality = "";
  address.value.barangay = "";
  address.value.city = "";
};
const onAddressRegionChange = () => {
  address.value.province = "";
  address.value.municipality = "";
  address.value.barangay = "";
};
const onAddressProvinceChange = () => {
  address.value.municipality = "";
  address.value.barangay = "";
};
const onAddressMunicipalityChange = () => {
  address.value.barangay = "";
};

// ─────────────────────────────────────────────
// OFFICE CASCADING HANDLERS
// ─────────────────────────────────────────────
const onOfficeCountryChange = () => {
  work.value.officeRegion = "";
  work.value.officeProvince = "";
  work.value.officeMunicipality = "";
  work.value.officeBarangay = "";
  work.value.officeCity = "";
};
const onOfficeRegionChange = () => {
  work.value.officeProvince = "";
  work.value.officeMunicipality = "";
  work.value.officeBarangay = "";
};
const onOfficeProvinceChange = () => {
  work.value.officeMunicipality = "";
  work.value.officeBarangay = "";
};
const onOfficeMunicipalityChange = () => {
  work.value.officeBarangay = "";
};

// ─────────────────────────────────────────────
// COMPUTED — CASCADING FILTERS
// ─────────────────────────────────────────────

// Birth place
const filteredProvinces = computed(() =>
  phProvinces.filter((p) => p.regionCode === birthRegion.value),
);
const filteredCities = computed(() =>
  phCities.filter((c) => c.provinceCode === birthProvince.value),
);
const filteredBarangays = computed(() => phBarangays.filter((b) => b.cityCode === birthCity.value));

// Address
const filteredAddressProvinces = computed(() =>
  phProvinces.filter((p) => p.regionCode === address.value.region),
);
const filteredAddressCities = computed(() =>
  phCities.filter((c) => c.provinceCode === address.value.province),
);
const filteredAddressBarangays = computed(() =>
  phBarangays.filter((b) => b.cityCode === address.value.municipality),
);

// Office
const filteredOfficeProvinces = computed(() =>
  phProvinces.filter((p) => p.regionCode === work.value.officeRegion),
);
const filteredOfficeCities = computed(() =>
  phCities.filter((c) => c.provinceCode === work.value.officeProvince),
);
const filteredOfficeBarangays = computed(() =>
  phBarangays.filter((b) => b.cityCode === work.value.officeMunicipality),
);

// ─────────────────────────────────────────────
// COMPUTED — FLAGS
// ─────────────────────────────────────────────
const selectedNationalityFlag = computed(
  () => nationalities.find((n) => n.name === user.value.citizenship)?.flag ?? "🌐",
);
const selectedBirthCountryFlag = computed(
  () => birthCountries.find((c) => c.code === birthCountry.value)?.flag ?? "🌐",
);
const selectedAddressCountryFlag = computed(
  () => birthCountries.find((c) => c.code === address.value.country)?.flag ?? "🌐",
);
const selectedOfficeCountryFlag = computed(
  () => birthCountries.find((c) => c.code === work.value.officeCountry)?.flag ?? "🌐",
);
const fatherCitizenshipFlag = computed(
  () => nationalities.find((n) => n.name === user.value.fatherCitizenship)?.flag ?? "🌐",
);
const motherCitizenshipFlag = computed(
  () => nationalities.find((n) => n.name === user.value.motherCitizenship)?.flag ?? "🌐",
);

// ─────────────────────────────────────────────
// COMPUTED — LOCATION BREADCRUMBS
// ─────────────────────────────────────────────
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

const addressLocationSummary = computed(() => {
  const parts = [];
  if (address.value.country) {
    const c = birthCountries.find((x) => x.code === address.value.country);
    if (c) parts.push(c.name);
  }
  if (address.value.country === "PH") {
    if (address.value.region)
      parts.push(phRegions.find((r) => r.code === address.value.region)?.name ?? "");
    if (address.value.province)
      parts.push(phProvinces.find((p) => p.code === address.value.province)?.name ?? "");
    if (address.value.municipality)
      parts.push(phCities.find((c) => c.code === address.value.municipality)?.name ?? "");
    if (address.value.barangay)
      parts.push(phBarangays.find((b) => b.code === address.value.barangay)?.name ?? "");
  } else if (address.value.city) {
    parts.push(address.value.city);
  }
  return parts.filter(Boolean).join(" › ");
});

const officeLocationSummary = computed(() => {
  const parts = [];
  if (work.value.officeCountry) {
    const c = birthCountries.find((x) => x.code === work.value.officeCountry);
    if (c) parts.push(c.name);
  }
  if (work.value.officeCountry === "PH") {
    if (work.value.officeRegion)
      parts.push(phRegions.find((r) => r.code === work.value.officeRegion)?.name ?? "");
    if (work.value.officeProvince)
      parts.push(phProvinces.find((p) => p.code === work.value.officeProvince)?.name ?? "");
    if (work.value.officeMunicipality)
      parts.push(phCities.find((c) => c.code === work.value.officeMunicipality)?.name ?? "");
    if (work.value.officeBarangay)
      parts.push(phBarangays.find((b) => b.code === work.value.officeBarangay)?.name ?? "");
  } else if (work.value.officeCity) {
    parts.push(work.value.officeCity);
  }
  return parts.filter(Boolean).join(" › ");
});

// ─────────────────────────────────────────────
// API — FETCH
// ─────────────────────────────────────────────
const fetchPersonal = async () => {
  try {
    isLoading.value = true;
    const { data } = await axios.get(
      "https://localhost:5000/api/PassportPersonalInformations/My-Profile",
      { headers: { Authorization: `Bearer ${Auth.token}` } },
    );

    user.value.passportPersonalInformationId = data.passportPersonalInformationId;
    user.value.lastName = data.lastName ?? "";
    user.value.firstName = data.firstName ?? "";
    user.value.middleName = data.middleName ?? "";
    user.value.nameExtension = data.suffix ?? "";
    user.value.birthdate = data.birthdate
      ? new Date(data.birthdate).toISOString().split("T")[0]
      : "";
    user.value.sex = data.gender ?? "";
    user.value.citizenship = data.nationality ?? "";
    user.value.civilStatusID = data.civilStatus ?? "";
    user.value.birthLegitimacy = data.birthLegitimacy ?? "";
    user.value.isAdoptee = data.isAdoptee ?? false;
    user.value.placeOfBirth = data.placeOfBirth ?? "";

    hasMiddleName.value = !!data.middleName;
    hasPSABirthCert.value = data.hasPSABirthcert ?? false;

    birthCountry.value = data.birthCountry ?? "PH";
    birthRegion.value = data.birthRegion ?? "";
    birthProvince.value = data.birthProvince ?? "";
    birthCity.value = data.birthCity ?? "";
    birthBarangay.value = data.birthBarangay ?? "";
  } catch (err) {
    console.log("fetchPersonal error:", err);
  } finally {
    isLoading.value = false;
  }
};

const fetchFamily = async () => {
  try {
    isLoading.value = true;
    const { data } = await axios.get("https://localhost:5000/api/Families/My-Family", {
      headers: { Authorization: `Bearer ${Auth.token}` },
    });

    const father = data.find((f) => f.relationship === "Father");
    const mother = data.find((f) => f.relationship === "Mother");

    // ======================
    // 👨 FATHER
    // ======================
    user.value.fatherSurname = father?.lastName ?? "";
    user.value.fatherFirstName = father?.firstName ?? "";
    user.value.fatherMiddleName = father?.middleName ?? "";
    user.value.fatherNameExtension = father?.suffix ?? "";
    user.value.fatherCitizenship = father?.citizenship ?? "";

    fatherLifeStatus.value = father ? (father.isAlive ? "alive" : "deceased") : "alive";
    fatherHasMiddleName.value = !!father?.middleName;

    // ======================
    // 👩 MOTHER
    // ======================
    user.value.motherId = mother?.familyId ?? null;
    user.value.motherFirstName = mother?.firstName ?? "";
    user.value.motherMiddleName = mother?.middleName ?? "";
    user.value.motherSurname = mother?.lastName ?? "";
    user.value.motherNameExtension = mother?.suffix ?? "";
    user.value.motherCitizenship = mother?.citizenship ?? "";

    motherLifeStatus.value = mother ? (mother.isAlive ? "alive" : "deceased") : "alive";

    motherHasMiddleName.value = !!mother?.middleName;

    // ✅ IMPORTANT: needed for update
    user.value.personalInfoId =
      father?.passportPersonalInformationId || mother?.passportPersonalInformationId || null;
  } catch (err) {
    console.log("fetchFamily error:", err);
  } finally {
    isLoading.value = false;
  }
};

const fetchContact = async () => {
  try {
    isLoading.value = true;

    const { data } = await axios.get(`https://localhost:5000/api/ContactInformation/My-Contact`, {
      headers: {
        Authorization: `Bearer ${Auth.token}`,
      },
    });

    // Store ID (important for update if needed later)
    contact.value.id = data.id;
    user.value.personalInfoId = data.passportPersonalInformationId;

    // Address fields
    contact.value.region = data.currentRegion ?? "";
    contact.value.province = data.currentProvince ?? "";
    contact.value.city = data.currentCityMunicipality ?? "";
    contact.value.barangay = data.currentBarangay ?? "";
    contact.value.postalCode = data.currentPostalCode ?? "";

    // Contact numbers
    contact.value.mobileNumber = data.personalMobileNumber ?? "";
    contact.value.landlineNumber = data.personalLandlineNumber ?? "";

    contact.value.email = data.email ?? "";
  } catch (err) {
    console.log("fetchContact error:", err);
  } finally {
    isLoading.value = false;
  }
};

const fetchWork = async () => {
  try {
    isLoading.value = true;

    const { data } = await axios.get(`https://localhost:5000/api/WorkInformation/My-Work`, {
      headers: {
        Authorization: `Bearer ${Auth.token}`,
      },
    });

    work.value.id = data.id;
    user.value.personalInfoId = data.passportPersonalInformationId;

    work.value.occupation = data.occupation ?? "";
    work.value.officeAddress = data.officeAddress ?? "";
    work.value.officeCountry = data.officeCountry ?? "";
    work.value.officeRegion = data.officeRegion ?? "";
    work.value.officeProvince = data.officeProvince ?? "";
    work.value.officeCityMunicipality = data.officeCityMunicipality ?? "";
    work.value.officePostalCode = data.officePostalCode ?? "";
    work.value.officeMobileNumber = data.officeMobileNumber ?? "";
    work.value.officeLandlineNumber = data.officeLandlineNumber ?? "";
  } catch (err) {
    console.log("fetchWork error:", err);
  } finally {
    isLoading.value = false;
  }
};

onMounted(async () => {
  await fetchPersonal();
  await fetchFamily();
  await fetchContact();
  await fetchWork();
});

// ------------------ PATCH Methods ------------------
const updatePersonal = async () => {
  try {
    isLoading.value = true;
    const payload = {
      passportPersonalInformationId: user.value.personalInfoId,
      lastName: user.value.lastName,
      firstName: user.value.firstName,
      middleName: hasMiddleName.value ? user.value.middleName : null,
      suffix: user.value.nameExtension || null,
      birthdate: user.value.birthdate || null,
      gender: user.value.sex || null,
      nationality: user.value.citizenship || null,
      civilStatus: user.value.civilStatusID || null,
      birthLegitimacy: user.value.birthLegitimacy,
      hasPSABirthcert: hasPSABirthCert.value,
      birthCountry: birthCountry.value,
      birthRegion: birthRegion.value,
      birthProvince: birthProvince.value,
      birthCity: birthCity.value,
      birthBarangay: birthBarangay.value,
    };
    await axios.patch("https://localhost:5000/api/PassportPersonalInformations", payload, {
      headers: { Authorization: `Bearer ${Auth.token}` },
    });
    dialogTitle.value = "Success";
    dialogMessage.value = "Personal info saved.";
    showDialog.value = true;
  } catch (err) {
    console.log("updatePersonal error:", err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to save personal info.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
  }
};

const updateFamily = async () => {
  try {
    isLoading.value = true;

    const payload = [
      {
        familyId: user.value.fatherId,
        passportPersonalInformationId: user.value.personalInfoId,

        firstName: user.value.fatherFirstName,
        middleName: fatherHasMiddleName.value ? user.value.fatherMiddleName : null,
        lastName: user.value.fatherSurname,
        suffix: user.value.fatherNameExtension,
        relationship: "Father",
        isAlive: fatherLifeStatus.value === "alive",
        citizenship: user.value.fatherCitizenship,
      },
      {
        familyId: user.value.motherId,
        passportPersonalInformationId: user.value.personalInfoId,

        firstName: user.value.motherFirstName,
        middleName: motherHasMiddleName.value ? user.value.motherMiddleName : null,
        lastName: user.value.motherSurname,
        suffix: user.value.motherNameExtension,
        relationship: "Mother",
        isAlive: motherLifeStatus.value === "alive",
        citizenship: user.value.motherCitizenship,
      },
    ];

    await axios.patch(`https://localhost:5000/api/Families`, payload, {
      headers: {
        Authorization: `Bearer ${Auth.token}`,
      },
    });

    dialogTitle.value = "Success";
    dialogMessage.value = "Family info saved.";
    showDialog.value = true;
  } catch (err) {
    console.log("updateFamily error:", err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to save family info.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
  }
};

const updateContact = async () => {
  try {
    isLoading.value = true;

    const payload = {
      id: contact.value.id,
      passportPersonalInformationId: user.value.personalInfoId,

      // Address
      currentStreet: address.value.street,
      currentAddressAbroad: address.value.abroad,
      currentCountry: address.value.country,
      currentRegion: address.value.country === "PH" ? address.value.region : null,
      currentProvince: address.value.country === "PH" ? address.value.province : null,
      currentCityMunicipality:
        address.value.country === "PH" ? address.value.municipality : address.value.city,
      currentBarangay: address.value.country === "PH" ? address.value.barangay : null,
      currentPostalCode: address.value.postal,

      // Contact numbers (stored as full formatted strings, matching fetch pattern)
      personalMobileNumber:
        contact.value.mobilePrefix || contact.value.mobileNumber
          ? `${contact.value.mobileCountry} ${contact.value.mobilePrefix}${contact.value.mobileNumber}`.trim()
          : null,
      personalLandlineNumber: contact.value.landlineNumber
        ? `${contact.value.landlineCountry} ${contact.value.landlineNumber}`.trim()
        : null,
    };

    await axios.patch("https://localhost:5000/api/ContactInformation", payload, {
      headers: { Authorization: `Bearer ${Auth.token}` },
    });

    dialogTitle.value = "Success";
    dialogMessage.value = "Contact info saved.";
    showDialog.value = true;
  } catch (err) {
    console.log("updateContact error:", err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to save contact info.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
  }
};

const updateWork = async () => {
  try {
    isLoading.value = true;

    const payload = {
      id: work.value.id,
      passportPersonalInformationId: user.value.personalInfoId,

      // Job details
      occupation: work.value.occupation,
      employer: work.value.employer,
      officeAddress: work.value.officeAddress,

      // Office location
      officeCountry: work.value.officeCountry,
      officeRegion: work.value.officeCountry === "PH" ? work.value.officeRegion : null,
      officeProvince: work.value.officeCountry === "PH" ? work.value.officeProvince : null,
      officeCityMunicipality:
        work.value.officeCountry === "PH" ? work.value.officeMunicipality : work.value.officeCity,
      officeBarangay: work.value.officeCountry === "PH" ? work.value.officeBarangay : null,
      officePostalCode: work.value.postalCode,

      // Work contact numbers
      officeMobileNumber:
        work.value.workMobilePrefix || work.value.workMobileNumber
          ? `${work.value.workMobileCountry} ${work.value.workMobilePrefix}${work.value.workMobileNumber}`.trim()
          : null,
      officeLandlineNumber: work.value.workLandlineNumber
        ? `${work.value.workLandlineCountry} ${work.value.workLandlineNumber}`.trim()
        : null,
    };

    await axios.patch("https://localhost:5000/api/WorkInformation", payload, {
      headers: { Authorization: `Bearer ${Auth.token}` },
    });

    dialogTitle.value = "Success";
    dialogMessage.value = "Work info saved.";
    showDialog.value = true;
  } catch (err) {
    console.log("updateWork error:", err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to save work info.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
  }
};

// ─────────────────────────────────────────────
// SAVE DISPATCHER
// ─────────────────────────────────────────────
const save = () => {
  showValidationErrors.value = true;
  if (activeTab.value === "Personal") updatePersonal();
  else if (activeTab.value === "Family") updateFamily();
  else if (activeTab.value === "Contact") updateContact();
  else if (activeTab.value === "Work") updateWork();
};

// ─────────────────────────────────────────────
// DIALOG
// ─────────────────────────────────────────────
const handleCloseDialog = () => {
  showDialog.value = false;
};

// ─────────────────────────────────────────────
// LIFECYCLE
// ─────────────────────────────────────────────
onMounted(async () => {
  await fetchPersonal();
  await fetchFamily();
});
</script>

<style>
/* ═══════════════════════════════════════════
     LAYOUT
  ═══════════════════════════════════════════ */
.app-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  grid-template-rows: auto 1fr;
  height: 100vh;
  background-color: #f4f7f9;
  overflow: hidden;
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

.dashboard-content {
  grid-column: 2;
  grid-row: 2;
  width: 100%;
  height: 100%;
  padding: 30px;
  box-sizing: border-box;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* ═══════════════════════════════════════════
     PAGE TITLE & IMPORTANT BOX
  ═══════════════════════════════════════════ */
.page-title {
  font-size: 1.8rem;
  color: #06195e;
  font-weight: 800;
  margin-bottom: 25px;
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

/* ═══════════════════════════════════════════
     TABS
  ═══════════════════════════════════════════ */
.tab-container {
  display: flex;
  gap: 2px;
  margin-bottom: 0;
  z-index: 2;
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

.tab-btn:hover:not(.active) {
  background: #e0e0e0;
  color: #004085;
}

.tab-btn.active {
  background: white;
  color: #06195e;
  border-top: 3px solid #06195e;
  padding-top: 9px;
}

.tab-wrapper {
  background: white;
  border-radius: 0 12px 12px 12px;
  padding: 25px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

/* ═══════════════════════════════════════════
     FORM WRAPPER & SECTIONS
  ═══════════════════════════════════════════ */
.form-wrapper {
  padding: 10px;
}

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

.pds-section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #06195e;
  margin: 0 0 2px;
}

.pds-section-sub {
  font-size: 0.75rem;
  color: #94a3b8;
  margin: 2px 0 0;
  font-weight: 400;
}

/* ═══════════════════════════════════════════
     SECTION ICON WRAPS
  ═══════════════════════════════════════════ */
.section-icon-wrap {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.section-icon-wrap.contact {
  background: #eff6ff;
  color: #3b82f6;
}

.section-icon-wrap.phone {
  background: #f0fdf4;
  color: #16a34a;
}

.section-icon-wrap.work {
  background: #faf5ff;
  color: #7c3aed;
}

.section-icon-wrap.office-loc {
  background: #fff7ed;
  color: #ea580c;
}

/* ═══════════════════════════════════════════
     FIELD GRID & FIELDS
  ═══════════════════════════════════════════ */
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

/* Full-width field within grid */
.contact-full {
  grid-column: 1 / -1;
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

.required-star {
  color: #e53935;
  font-weight: 800;
}

/* ═══════════════════════════════════════════
     INPUTS
  ═══════════════════════════════════════════ */
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

/* ═══════════════════════════════════════════
     GENDER TOGGLE
  ═══════════════════════════════════════════ */
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

/* ═══════════════════════════════════════════
     YES/NO TOGGLES
  ═══════════════════════════════════════════ */
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

/* ═══════════════════════════════════════════
     N/A PILL
  ═══════════════════════════════════════════ */
.pds-na-pill {
  padding: 9px 12px;
  background: #f1f5f9;
  border: 1.5px dashed #cbd5e0;
  border-radius: 8px;
  font-size: 0.82rem;
  color: #a0aec0;
  font-style: italic;
}

/* ═══════════════════════════════════════════
     FLAG SELECT
  ═══════════════════════════════════════════ */
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

/* ═══════════════════════════════════════════
     LOCATION BREADCRUMB
  ═══════════════════════════════════════════ */
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

/* ═══════════════════════════════════════════
     LIFE STATUS TOGGLE (FAMILY)
  ═══════════════════════════════════════════ */
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

/* ═══════════════════════════════════════════
     DECEASED STATUS NOTICE
  ═══════════════════════════════════════════ */
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

/* ═══════════════════════════════════════════
     MAIDEN TAG
  ═══════════════════════════════════════════ */
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

/* ═══════════════════════════════════════════
     PHONE CARDS (CONTACT & WORK)
  ═══════════════════════════════════════════ */
.contact-phone-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 16px;
}

.phone-card {
  background: #f8fafc;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  padding: 16px 18px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  transition:
    border-color 0.18s,
    box-shadow 0.18s;
}

.phone-card:focus-within {
  border-color: #06195e;
  box-shadow: 0 0 0 3px rgba(6, 25, 94, 0.08);
}

.phone-card-label {
  display: flex;
  align-items: center;
}

.phone-type-badge {
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  padding: 3px 10px;
  border-radius: 20px;
}

.phone-type-badge.mobile {
  background: #dbeafe;
  color: #1d4ed8;
}

.phone-type-badge.landline {
  background: #d1fae5;
  color: #065f46;
}

.phone-input-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.phone-country-select {
  padding: 8px 10px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  font-size: 0.82rem;
  background: #fff;
  min-width: 105px;
  cursor: pointer;
  height: 38px;
  appearance: none;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
  background-position: right 6px center;
  background-repeat: no-repeat;
  background-size: 14px;
  padding-right: 26px;
}

.phone-country-select:focus {
  outline: none;
  border-color: #06195e;
  box-shadow: 0 0 0 3px rgba(6, 25, 94, 0.1);
}

.phone-segment {
  max-width: 72px;
  text-align: center;
  padding: 8px 10px !important;
  height: 38px;
}

.phone-main {
  flex: 1;
  padding: 8px 10px !important;
  height: 38px;
}

.phone-dash {
  color: #94a3b8;
  font-weight: 700;
  font-size: 1rem;
  flex-shrink: 0;
}

.phone-preview {
  font-size: 0.78rem;
  color: #3b82f6;
  font-weight: 600;
  padding: 4px 10px;
  background: #eff6ff;
  border-radius: 6px;
  width: fit-content;
  letter-spacing: 0.3px;
}

/* ═══════════════════════════════════════════
     BUTTONS
  ═══════════════════════════════════════════ */
.btn {
  background-color: #007bff;
  color: white;
  padding: 10px 15px;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  border: 1px solid #007bff;
  transition: all 0.2s;
}

.btn:hover {
  background-color: #0056b3;
  border-color: #0056b3;
}

.save-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.button-group-row {
  display: flex;
  margin-top: 20px;
}

/* ═══════════════════════════════════════════
     TRANSITIONS
  ═══════════════════════════════════════════ */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition:
    opacity 0.3s ease,
    transform 0.3s ease;
}

.fade-slide-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* ═══════════════════════════════════════════
     RESPONSIVE
  ═══════════════════════════════════════════ */
@media (max-width: 1200px) {
  .app-layout {
    grid-template-columns: 20vw 1fr;
  }
}

@media (max-width: 992px) {
  .app-layout {
    grid-template-columns: 80px 1fr;
  }

  .tab-container {
    flex-wrap: wrap;
  }

  .tab-btn {
    flex: 1 1 auto;
    border: 1px solid #e0e0e0;
    border-radius: 6px;
    margin: 5px;
  }
}

@media (max-width: 768px) {
  .app-layout {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto 1fr;
  }

  .leftMenu,
  .header,
  .dashboard-content {
    grid-column: 1;
  }

  .header {
    grid-row: 1;
  }

  .dashboard-content {
    grid-row: 2;
    padding: 20px 15px;
  }

  .leftMenu {
    grid-row: 3;
    width: 100%;
    min-height: auto;
  }

  .phone-input-row {
    flex-wrap: wrap;
  }

  .phone-segment {
    max-width: 100%;
  }
}
</style>
