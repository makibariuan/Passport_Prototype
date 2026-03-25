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
                      <select v-model="user.civilStatusID"
                              class="pds-input"
                              :class="{ 'pds-input-error': showValidationErrors && !user.civilStatusID }">
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
                  <button @click="save" class="btn" style="margin-left: auto; width: 200px">
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
                    <button @click="save" class="btn" style="margin-left: auto; width: 200px">
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
                <div style="margin-bottom: 30px">
                  <div style="display: flex; align-items: center; gap: 15px; margin-bottom: 20px">
                    <p style="margin: 0; font-weight: bold; white-space: nowrap">
                      Personal Contact & Address
                    </p>
                  </div>

                  <div style="display: flex; flex-direction: column; gap: 12px">
                    <label class="form-row">
                      <span>Current Address</span>
                      <input class="auth-input" v-model="address.street" />
                    </label>
                    <label class="form-row">
                      <span>Address Abroad</span>
                      <input class="auth-input" v-model="address.abroad" />
                    </label>
                    <label class="form-row">
                      <span>Current Country</span>
                      <select class="auth-select" v-model="address.country">
                        <option value="">Select Country</option>
                      </select>
                    </label>
                    <label class="form-row">
                      <span>Current Region</span>
                      <select class="auth-select" v-model="address.region">
                        <option value="">Select Region</option>
                      </select>
                    </label>
                    <label class="form-row">
                      <span>Current Province</span>
                      <select class="auth-select" v-model="address.province">
                        <option value="">Select Province</option>
                      </select>
                    </label>
                    <label class="form-row">
                      <span>Current Municipality</span>
                      <select class="auth-select" v-model="address.municipality">
                        <option value="">Select Municipality</option>
                      </select>
                    </label>
                    <label class="form-row">
                      <span>Current Barangay</span>
                      <select class="auth-select" v-model="address.barangay">
                        <option value="">Select Barangay</option>
                      </select>
                    </label>
                    <label class="form-row">
                      <span>Current Postal Code</span>
                      <input class="auth-input" v-model="address.postal" />
                    </label>
                  </div>

                  <!-- Personal Mobile -->
                  <label class="form-row" style="margin-top: 24px">
                    <span>Personal Mobile</span>
                    <div class="phone-input-group">
                      <select class="phone-flag" v-model="contact.mobileCountry">
                        <option value="+63">🇵🇭 +63</option>
                        <option value="+1">🇺🇸 +1</option>
                        <option value="+44">🇬🇧 +44</option>
                      </select>
                      <input
                        class="auth-input phone-prefix"
                        placeholder="906"
                        v-model="contact.mobilePrefix"
                        maxlength="4"
                      />
                      <input
                        class="auth-input phone-number"
                        placeholder="1234567"
                        v-model="contact.mobileNumber"
                      />
                    </div>
                  </label>

                  <!-- Personal Landline -->
                  <label class="form-row" style="margin-top: 16px">
                    <span>Personal Landline</span>
                    <div class="phone-input-group">
                      <select class="phone-flag" v-model="contact.landlineCountry">
                        <option value="+63">🇵🇭 +63</option>
                        <option value="+1">🇺🇸 +1</option>
                        <option value="+44">🇬🇧 +44</option>
                      </select>
                      <input
                        class="auth-input phone-number"
                        placeholder="02-123-4567"
                        v-model="contact.landlineNumber"
                      />
                    </div>
                  </label>

                  <div class="button-group-row">
                    <button @click="save" class="btn" style="margin-left: auto; width: 200px">
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
                  <div style="margin-bottom: 30px">
                    <div style="display: flex; align-items: center; gap: 15px; margin-bottom: 20px">
                      <p style="margin: 0; font-weight: bold; white-space: nowrap">
                        Work Information
                      </p>
                    </div>

                    <div style="display: flex; flex-direction: column; gap: 12px">
                      <label class="form-row">
                        <span>Occupation</span>
                        <input class="auth-input" v-model="work.occupation" />
                      </label>
                      <label class="form-row">
                        <span>Office Address</span>
                        <input class="auth-input" v-model="work.officeAddress" />
                      </label>
                      <label class="form-row">
                        <span>Office Country</span>
                        <select class="auth-select" v-model="work.officeCountry">
                          <option value="">Select Country</option>
                        </select>
                      </label>
                      <label class="form-row">
                        <span>Office Region</span>
                        <select class="auth-select" v-model="work.officeRegion">
                          <option value="">Select Region</option>
                        </select>
                      </label>
                      <label class="form-row">
                        <span>Office Province</span>
                        <select class="auth-select" v-model="work.officeProvince">
                          <option value="">Select Province</option>
                        </select>
                      </label>
                      <label class="form-row">
                        <span>Office Municipality</span>
                        <select class="auth-select" v-model="work.officeMunicipality">
                          <option value="">Select Municipality</option>
                        </select>
                      </label>
                      <label class="form-row">
                        <span>Office Barangay</span>
                        <select class="auth-select" v-model="work.officeBarangay">
                          <option value="">Select Barangay</option>
                        </select>
                      </label>
                      <label class="form-row">
                        <span>Office Postal Code</span>
                        <input class="auth-input" v-model="work.postalCode" />
                      </label>
                    </div>

                    <!-- Work Mobile -->
                    <label class="form-row" style="margin-top: 24px">
                      <span>Work Mobile</span>
                      <div class="phone-input-group">
                        <select class="phone-flag" v-model="contact.mobileCountry">
                          <option value="+63">🇵🇭 +63</option>
                          <option value="+1">🇺🇸 +1</option>
                          <option value="+44">🇬🇧 +44</option>
                        </select>
                        <input
                          class="auth-input phone-prefix"
                          placeholder="906"
                          v-model="contact.mobilePrefix"
                          maxlength="4"
                        />
                        <input
                          class="auth-input phone-number"
                          placeholder="1234567"
                          v-model="contact.mobileNumber"
                        />
                      </div>
                    </label>

                    <!-- Work Landline -->
                    <label class="form-row" style="margin-top: 16px">
                      <span>Work Landline</span>
                      <div class="phone-input-group">
                        <select class="phone-flag" v-model="contact.landlineCountry">
                          <option value="+63">🇵🇭 +63</option>
                          <option value="+1">🇺🇸 +1</option>
                          <option value="+44">🇬🇧 +44</option>
                        </select>
                        <input
                          class="auth-input phone-number"
                          placeholder="02-123-4567"
                          v-model="contact.landlineNumber"
                        />
                      </div>
                    </label>

                    <div class="button-group-row">
                      <button @click="save" class="btn" style="margin-left: auto; width: 200px">
                        Save Progress
                      </button>
                    </div>
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

const showDialog = ref(false);
const dialogTitle = ref("");
const dialogMessage = ref("");
const isLoading = ref(false);
const activeTab = ref("Personal");
  const userId = 5;
  const Auth = useAuthStore();

// ------------------ State ------------------
const user = ref({
  lastName: "",
  firstName: "",
  middleName: "",
  nameExtension: "",
  dateOfBirth: "",
  sex: "",
  citizenship: "",
  civilStatusID: "",
  birthLegitimacy: "",
  isAdoptee: false,
  placeOfBirth: "",
  // family fields live here — the template binds to these
  fatherSurname: "",
  fatherFirstName: "",
  fatherMiddleName: "",
  fatherNameExtension: "",
  fatherCitizenship: "",
  motherSurname: "",
  motherFirstName: "",
  motherMiddleName: "",
  motherNameExtension: "",
  motherCitizenship: "",
});

const family = ref({
  relationship: "",
  lastName: "",
  firstName: "",
  middleName: "",
  extension: "",
  citizenship: "",
});

const contact = ref({
  mobile: "",
  landline: "",
  email: "",
  mobileCountry: "+63",
  mobilePrefix: "",
  mobileNumber: "",
  landlineCountry: "+63",
  landlineNumber: "",
});

const work = ref({
  occupation: "",
  officeAddress: "",
  officeCountry: "",
  officeRegion: "",
  officeProvince: "",
  officeMunicipality: "",
  officeBarangay: "",
  postalCode: "",
});

const address = ref({
  street: "",
  abroad: "",
  city: "",
  province: "",
  region: "",
  municipality: "",
  barangay: "",
  postal: "",
  postalCode: "",
  country: "",
});

const showValidationErrors = ref(false);

// ── Personal tab extras ──────────────────────────────────────────
const hasMiddleName = ref(true);
const hasPSABirthCert = ref(false);
const civilStatuses = ref([]);

// Birth location cascading refs
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

// ── Nationality list ────────────────────────
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

// ── Family tab extras ────────────────────────────
const fatherLifeStatus = ref("alive");
const motherLifeStatus = ref("alive");
const fatherHasMiddleName = ref(true);
const motherHasMiddleName = ref(true);

// Flag computed helpers for parent citizenship selects
const fatherCitizenshipFlag = computed(
  () => nationalities.find((n) => n.name === user.value.fatherCitizenship)?.flag ?? "🌐",
);
const motherCitizenshipFlag = computed(
  () => nationalities.find((n) => n.name === user.value.motherCitizenship)?.flag ?? "🌐",
);

// ------------------ GET Methods ------------------
const fetchPersonal = async () => {
  try {
    isLoading.value = true;
    const { data } = await axios.get(
      `https://localhost:5000/api/PassportPersonalInformations/My-Profile`, {
        headers: {
          Authorization: `Bearer ${Auth.token}`
      }
    }
    );

    user.value.lastName = data.lastName ?? "";
    user.value.firstName = data.firstName ?? "";
    user.value.middleName = data.middleName ?? "";
    user.value.nameExtension = data.suffix ?? "";

    user.value.dateOfBirth = data.birthdate
      ? new Date(data.birthdate)
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

      const { data } = await axios.get(
        `https://localhost:5000/api/Families/My-Family`,
        {
          headers: {
            Authorization: `Bearer ${Auth.token}`
          }
        }
      );

      // ✅ Extract from array
      const father = data.find(f => f.relationship === "Father");
      const mother = data.find(f => f.relationship === "Mother");

      // ======================
      // 👨 FATHER
      // ======================
      user.value.fatherSurname = father?.lastName ?? "";
      user.value.fatherFirstName = father?.firstName ?? "";
      user.value.fatherMiddleName = father?.middleName ?? "";
      user.value.fatherNameExtension = father?.suffix ?? "";
      user.value.fatherCitizenship = father?.citizenship ?? "";

      fatherLifeStatus.value = father
        ? (father.isAlive ? "alive" : "deceased")
        : "alive";

      fatherHasMiddleName.value = !!father?.middleName;

      // ======================
      // 👩 MOTHER
      // ======================
      user.value.motherSurname = mother?.lastName ?? "";
      user.value.motherFirstName = mother?.firstName ?? "";
      user.value.motherMiddleName = mother?.middleName ?? "";
      user.value.motherNameExtension = mother?.suffix ?? ""; // ✅ FIXED
      user.value.motherCitizenship = mother?.citizenship ?? "";

      motherLifeStatus.value = mother
        ? (mother.isAlive ? "alive" : "deceased")
        : "alive";

      motherHasMiddleName.value = !!mother?.middleName;

    } catch (err) {
      console.log("fetchFamily error:", err);
    } finally {
      isLoading.value = false;
    }
  };

onMounted(async () => {
  await fetchPersonal();
  await fetchFamily();
});

// ------------------ PATCH Methods ------------------
const updatePersonal = async () => {
  try {
    isLoading.value = true;

    const payload = {
      lastName: user.value.lastName,
      firstName: user.value.firstName,
      middleName: hasMiddleName.value ? user.value.middleName : null,
      suffix: user.value.nameExtension || null,

      birthdate: user.value.dateOfBirth || null,
      gender: user.value.sex || null,
      nationality: user.value.citizenship || null,
      civilStatus: user.value.civilStatusID || null, // ⚠️ only if string

      birthLegitimacy: user.value.birthLegitimacy,

      hasPSABirthcert: hasPSABirthCert.value,

      birthCountry: birthCountry.value,
      birthRegion: birthRegion.value,
      birthProvince: birthProvince.value,
      birthCity: birthCity.value,
      birthBarangay: birthBarangay.value
    };

    await axios.patch(`https://localhost:5000/api/PassportPersonalInformations/Update-Profile`, payload,
      {
        headers: {
          Authorization: `Bearer ${Auth.token}`
      }
    });
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

const updateFamily = async () => {
  try {
    isLoading.value = true;
    const payload = {
      father: {
        surname: user.value.fatherLastName,
        firstName: user.value.fatherFirstName,
        middleName: fatherHasMiddleName.value ? user.value.fatherMiddleName : null,
        nameExtension: user.value.fatherNameExtension,
        citizenship: user.value.fatherCitizenship,
        lifeStatus: fatherLifeStatus.value,
      },
      mother: {
        surname: user.value.motherLastName,
        firstName: user.value.motherFirstName,
        middleName: motherHasMiddleName.value ? user.value.motherMiddleName : null,
        nameExtension: user.value.motherNameExtension,
        citizenship: user.value.motherCitizenship,
        lifeStatus: motherLifeStatus.value,
      },
    };
    await axios.patch(`https://localhost:5000/api/Families/${userId}`, payload);
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

const updateContact = async () => {
  try {
    isLoading.value = true;
    await axios.patch("https://localhost:5000/user/contact", {
      mobile: `${contact.value.mobileCountry}${contact.value.mobilePrefix}${contact.value.mobileNumber}`,
      landline: `${contact.value.landlineCountry}${contact.value.landlineNumber}`,
      address: address.value,
    });
    dialogTitle.value = "Success";
    dialogMessage.value = "Contact info saved.";
    showDialog.value = true;
  } catch (err) {
    console.log("error: ", err);
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
    await axios.patch("https://localhost:5000/user/work", { ...work.value });
    dialogTitle.value = "Success";
    dialogMessage.value = "Work info saved.";
    showDialog.value = true;
  } catch (err) {
    console.log("error: ", err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to save work info.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
  }
};

const save = () => {
  showValidationErrors.value = true;
  if (activeTab.value === "Personal") updatePersonal();
  else if (activeTab.value === "Family") updateFamily();
  else if (activeTab.value === "Contact") updateContact();
  else if (activeTab.value === "Work") updateWork();
};

const handleCloseDialog = () => {
  showDialog.value = false;
};
</script>

<style>
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

.page-title {
  font-size: 1.8rem;
  color: #06195e;
  font-weight: 800;
  margin-bottom: 25px;
}

.sub-title {
  font-size: 1.5rem;
  color: #06195e;
  padding-bottom: 10px;
  border-bottom: 2px solid #f0f4f8;
  font-weight: 700;
}

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

.pds-na-pill {
  padding: 9px 12px;
  background: #f1f5f9;
  border: 1.5px dashed #cbd5e0;
  border-radius: 8px;
  font-size: 0.82rem;
  color: #a0aec0;
  font-style: italic;
}

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

.button-group-row {
  display: flex;
  margin-top: 20px;
}

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

.required-star {
  color: #e53935;
  font-weight: 800;
}

/* Contact / Work form styles */
.form-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 4px;
}

.form-row span {
  min-width: 160px;
  font-weight: 500;
  color: #374151;
  font-size: 14px;
}

.auth-input,
.auth-select,
.phone-flag {
  padding: 10px 14px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  background-color: #f9fafb;
  transition: all 0.2s ease;
  height: 44px;
  box-sizing: border-box;
}

.auth-input {
  flex: 1;
}

.auth-select {
  flex: 1;
  appearance: none;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
  background-position: right 12px center;
  background-repeat: no-repeat;
  background-size: 16px;
  padding-right: 40px;
  cursor: pointer;
}

.phone-input-group {
  display: flex;
  gap: 8px;
  flex: 1;
  align-items: center;
}

.phone-flag {
  min-width: 100px;
  padding-right: 32px;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
  background-position: right 8px center;
  background-repeat: no-repeat;
  background-size: 16px;
  cursor: pointer;
  appearance: none;
}

.phone-prefix {
  max-width: 80px;
  text-align: center;
}

.phone-number {
  flex: 1;
}

.auth-input:focus,
.auth-select:focus,
.phone-flag:focus {
  outline: none;
  border-color: #3b82f6;
  background-color: #eef2ff;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* Responsive */
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

  .form-row {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .form-row span {
    min-width: auto;
    font-weight: bold;
  }

  .phone-input-group {
    flex-direction: column;
    width: 100%;
  }
}
</style>
