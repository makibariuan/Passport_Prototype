<template>
  <div class="app-layout">
    <LeftMenu class="leftMenu" />
    <!--<Header title="Application" class="header" />-->

    <div class="dashboard-content">
      <h2 class="page-title">Passport Application</h2>

      <!-- ═══════════════════════════════════════════════════════════ -->
      <!-- PRE-STEP 0 — Terms & Conditions                           -->
      <!-- ═══════════════════════════════════════════════════════════ -->
      <div v-if="preStep === 'terms'" class="pre-step-wrapper">
        <div class="terms-card">
          <h2 class="terms-title">
            Welcome to the<br /><strong>DFA Online Appointment System</strong>
          </h2>
          <p class="terms-subtitle">
            Review all fields in the online form carefully and provide complete and accurate
            information
          </p>

          <div class="reminder-box">
            <span class="reminder-label">Reminder</span>
            <p>
              Applicants are advised to use only Google or Yahoo email accounts in securing an
              appointment to avoid any technical incompatibilities. Email address restrictions and
              accessibility may vary on your access location, country and/or email servers.
            </p>
          </div>

          <h3 class="terms-section-title">Terms and conditions</h3>
          <p>
            This appointment and scheduling system allocates slots on a first come, first served
            basis.
          </p>
          <p>
            Users accept the responsibility for supplying, checking, and verifying the accuracy and
            correctness of the information they provide on this system in connection with their
            application. Incorrect or inaccurate information supplied may result in forfeiture of
            passport application.
          </p>
          <p>
            For sites utilizing the ePayment System, all fees are non-refundable. Fees shall be
            forfeited for applicants who fail to show up on their confirmed appointment, applicants
            who cancel their appointment, applicants whose application was rejected due to
            inconsistency and/or incorrect information, and applicants who present discrepant and/or
            spurious documents.
          </p>

          <div class="consent-row">
            <label class="consent-label">
              <input v-model="consentChecked" type="checkbox" class="consent-checkbox" />
              <span class="consent-text">
                By proceeding with this application, I understand that I am signifying my
                unequivocal consent to the disclosure, collection, and use of my personal
                information and the data required under the Philippine Passport Act as amended and
                its Implementing Rules and Regulations. My consent effectively constitutes a waiver
                of any and all privacy rights pertaining to the disclosure, collection, and use of
                my personal information and data under the specific terms and condition of the DFA
                Online Passport Appointment System Website's Privacy Policy, the Data Privacy Act of
                2012 and its Implementing Rules and Regulations and other pertinent DFA rules,
                regulations, policies on the matter.
              </span>
            </label>
          </div>

          <div class="terms-actions">
            <button class="btn btn-continue-existing" @click="continueExisting">
              ↻ Continue existing application
            </button>
            <div class="terms-new-actions">
              <button
                class="btn btn-individual"
                :disabled="!consentChecked"
                @click="startIndividual"
              >
                👤 Individual application
              </button>
              <div class="group-btn-wrap">
                <button class="btn btn-group" :disabled="!consentChecked" @click="startGroup">
                  👥 Group application
                </button>
                <p class="group-note">At least two profiles are needed for group appointment</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ═══════════════════════════════════════════════════════════ -->
      <!-- PRE-STEP 1 — Select Profile Modal                         -->
      <!-- ═══════════════════════════════════════════════════════════ -->
      <div v-else-if="preStep === 'selectProfile'" class="pre-step-wrapper">
        <div class="modal-overlay">
          <div class="modal-card">
            <div class="modal-icon">
              <svg width="48" height="48" viewBox="0 0 48 48" fill="none">
                <circle cx="24" cy="18" r="8" stroke="#06195e" stroke-width="2.5" fill="none" />
                <path
                  d="M8 42c0-8.837 7.163-16 16-16s16 7.163 16 16"
                  stroke="#06195e"
                  stroke-width="2.5"
                  stroke-linecap="round"
                  fill="none"
                />
              </svg>
            </div>
            <h3 class="modal-title">Select Profile</h3>
            <p class="modal-subtitle">Select the profile of the applicant to continue</p>

            <div class="profile-list">
              <label
                v-for="profile in profiles"
                :key="profile.id"
                class="profile-option"
                :class="{ selected: selectedProfile === profile.id }"
              >
                <input
                  v-model="selectedProfile"
                  type="radio"
                  :value="profile.id"
                  class="profile-radio-input"
                />
                <svg class="profile-icon" width="20" height="20" viewBox="0 0 20 20" fill="none">
                  <circle cx="10" cy="7" r="3.5" stroke="#06195e" stroke-width="1.5" fill="none" />
                  <path
                    d="M3 18c0-3.866 3.134-7 7-7s7 3.134 7 7"
                    stroke="#06195e"
                    stroke-width="1.5"
                    stroke-linecap="round"
                    fill="none"
                  />
                </svg>
                <span class="profile-name">{{ profile.name }}</span>
                <span class="profile-radio">
                  <span v-if="selectedProfile === profile.id" class="radio-dot" />
                </span>
              </label>
            </div>

            <button
              class="btn btn-proceed"
              :disabled="!selectedProfile"
              @click="proceedWithProfile"
            >
              Proceed
            </button>
          </div>
        </div>
      </div>

      <!-- ═══════════════════════════════════════════════════════════ -->
      <!-- PRE-STEP 2 — Appointment Notice Modal                     -->
      <!-- ═══════════════════════════════════════════════════════════ -->
      <div v-else-if="preStep === 'appointmentNotice'" class="pre-step-wrapper">
        <div class="modal-overlay">
          <div class="modal-card notice-card">
            <h3 class="modal-title">Appointment Notice</h3>
            <p class="modal-subtitle">
              You are attempting to secure an appointment through the Special Site Facility of the
              DFA.
            </p>

            <ol class="notice-list">
              <li>
                Appointment booked at any Temporary Off-Site Passport Service (TOPS) is
                non-transferable and cannot be rescheduled. If an applicant has an existing
                appointment at any TOPS or Consular Office, the existing TOPS or Consular Office
                appointment must first be cancelled. A new appointment can then be booked and paid.
              </li>
              <li>
                Applicants have a choice to either use a courier delivery service or pick-up their
                new passport. Applicants who wish to pick-up their passports will have to go to the
                Supervising Consular Office of the TOPS Site to personally claim their passports.
                Please click
                <a href="#" class="notice-link">here</a> for the requirements for passport release.
              </li>
              <li>
                Applicants who wish to have their passports released via courier must agree to have
                their current passport cancelled on-site.
              </li>
              <li>
                There is no expedited processing when you apply at a TOPS facility. Processing time
                is between 12–16 working days. The delivery of passports via courier will add 3–5
                working days to the passport turnaround time.
              </li>
              <li>
                Before you proceed, make sure that you have your current passport and credit/debit
                card details on hand if paying online. Only charges shown during the online booking
                process are to be paid. No other fee will be paid on-site, nor should applicants pay
                extra fees to obtain an appointment.
              </li>
            </ol>

            <div class="notice-actions">
              <button class="btn btn-cancel" @click="cancelNotice">Cancel</button>
              <button class="btn btn-agree" @click="agreeNotice">I Agree</button>
            </div>
          </div>
        </div>
      </div>

      <!-- ═══════════════════════════════════════════════════════════ -->
      <!-- MAIN STEPPER — shown after all pre-steps are done         -->
      <!-- ═══════════════════════════════════════════════════════════ -->
      <template v-else>
        <div class="pds-top-bar">
          <div class="stepper">
            <div
              v-for="(tab, index) in tabs"
              :key="tab"
              :class="['step', { active: currentStep === index, done: currentStep > index }]"
            >
              <div class="step-circle">
                <span v-if="currentStep > index">✓</span>
                <span v-else>{{ index + 1 }}</span>
              </div>
              <div class="step-label">{{ tab }}</div>
            </div>
          </div>
        </div>

        <div class="tab-wrapper">
          <div class="tab-content">
            <transition name="fade-slide" mode="out-in">
              <div :key="currentStep" class="form-wrapper">
                <!-- ═══════════════════════════════════════════════════ -->
                <!-- TAB 0 — Site Location & Schedule                   -->
                <!-- ═══════════════════════════════════════════════════ -->
                <div v-if="currentStep === 0">
                  <!-- ── Site Location ── -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title site-location-title">Site location</h3>
                    </div>

                    <div class="site-location-grid">
                      <!-- LEFT: dropdowns -->
                      <div class="site-dropdowns">
                        <div class="form-group">
                          <label class="form-label">Region <span class="required">*</span></label>
                          <div class="select-wrap">
                            <select v-model="siteForm.region" class="form-select">
                              <option value="asia-pacific">Asia Pacific</option>
                              <option value="americas">Americas</option>
                              <option value="europe">Europe</option>
                              <option value="middle-east">Middle East</option>
                            </select>
                            <span class="select-arrow">▾</span>
                          </div>
                        </div>

                        <div class="form-group">
                          <label class="form-label">
                            Country/Special Administrative Region
                            <span class="required">*</span>
                          </label>
                          <div class="select-wrap">
                            <select v-model="siteForm.country" class="form-select">
                              <option value="philippines">Philippines</option>
                              <option value="japan">Japan</option>
                              <option value="usa">United States</option>
                            </select>
                            <span class="select-arrow">▾</span>
                          </div>
                        </div>

                        <div class="form-group">
                          <label class="form-label">Site <span class="required">*</span></label>
                          <div class="select-wrap">
                            <select v-model="siteForm.site" class="form-select">
                              <option value="dfa-aseana">DFA Manila (Aseana)</option>
                              <option value="dfa-san-fernando">DFA San Fernando</option>
                              <option value="dfa-cebu">DFA Cebu</option>
                              <option value="dfa-davao">DFA Davao</option>
                            </select>
                            <span class="select-arrow">▾</span>
                          </div>
                        </div>
                      </div>

                      <!-- RIGHT: office info -->
                      <div class="site-info-card">
                        <div class="site-info-row">
                          <span class="site-info-label">Office Name</span>
                          <span class="site-info-value">{{ selectedSiteInfo.name }}</span>
                        </div>
                        <div class="site-info-row">
                          <span class="site-info-label">Office Address</span>
                          <span class="site-info-value">{{ selectedSiteInfo.address }}</span>
                        </div>
                        <div class="site-info-row">
                          <span class="site-info-label">Contact Number</span>
                          <span class="site-info-value">{{ selectedSiteInfo.contact }}</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- ── Schedule ── -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title site-location-title">Schedule</h3>
                    </div>

                    <div class="schedule-notice">
                      Please be advised that your chosen time slot is reserved for 30 minutes.
                    </div>

                    <div class="selected-schedule-row">
                      <span class="selected-schedule-label">Selected Schedule</span>
                      <span class="selected-schedule-value">{{ formatSelectedSchedule }}</span>
                    </div>

                    <div class="date-time-label">Date and time</div>

                    <div class="earliest-badge">
                      Earliest available appointment: {{ earliestAppointmentLabel }}
                    </div>

                    <!-- Calendar + Time Slots side by side -->
                    <div class="calendar-time-grid">
                      <!-- Calendar -->
                      <div class="calendar-wrapper">
                        <div class="calendar-header">
                          <button class="cal-nav" @click="prevMonth">‹</button>
                          <span class="cal-month-label">{{ calMonthLabel }}</span>
                          <button class="cal-nav" @click="nextMonth">›</button>
                        </div>

                        <div class="cal-weekdays">
                          <span v-for="d in ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa']" :key="d">{{
                            d
                          }}</span>
                        </div>

                        <div class="cal-days">
                          <span
                            v-for="n in calStartBlank"
                            :key="'blank-' + n"
                            class="cal-day blank"
                          />
                          <button
                            v-for="day in calDaysInMonth"
                            :key="day.date"
                            class="cal-day"
                            :class="{
                              available: day.available,
                              unavailable: !day.available,
                              selected: selectedDate === day.date,
                              today: day.isToday,
                            }"
                            :disabled="!day.available"
                            @click="selectDate(day.date)"
                          >
                            {{ day.num }}
                          </button>
                        </div>
                      </div>

                      <!-- Time Slots -->
                      <div class="time-slots-wrapper">
                        <div
                          v-for="slot in timeSlots"
                          :key="slot.time"
                          class="time-slot-row"
                          :class="{ selected: selectedTime === slot.time }"
                          @click="selectedTime = slot.time"
                        >
                          <label class="time-slot-label">
                            <input
                              v-model="selectedTime"
                              type="radio"
                              :value="slot.time"
                              class="time-radio-input"
                            />
                            <span class="time-radio-circle">
                              <span v-if="selectedTime === slot.time" class="time-radio-dot" />
                            </span>
                            <span class="time-slot-text">{{ slot.time }}</span>
                          </label>
                          <span
                            class="slots-badge"
                            :class="slot.slots > 0 ? 'slots-available' : 'slots-full'"
                          >
                            {{
                              slot.slots > 0
                                ? `Available slots: ${slot.slots}`
                                : "No slots available"
                            }}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- ═══════════════════════════════════════════════════ -->
                <!-- TAB 1 — Application Type                           -->
                <!-- ═══════════════════════════════════════════════════ -->
                <div v-else-if="currentStep === 1">
                  <div class="apptype-schedule-bar">
                    <div class="apptype-schedule-info">
                      <span class="apptype-schedule-label">Selected Schedule</span>
                      <span class="apptype-schedule-value">{{ formatSelectedSchedule }}</span>
                    </div>
                  </div>

                  <div class="apptype-notice">
                    Please bring your proof of Philippine citizenship by election, naturalization,
                    re-acquisition on the date of your personal appearance.
                  </div>

                  <div class="pds-section">
                    <div class="apptype-row">
                      <div class="form-group">
                        <label class="form-label"
                          >Application Type <span class="required">*</span></label
                        >
                        <div class="select-wrap">
                          <select v-model="appTypeForm.applicationType" class="form-select">
                            <option value="" disabled></option>
                            <option value="new">New Application</option>
                            <option value="renewal">Renewal</option>
                            <option value="replacement">Replacement</option>
                          </select>
                          <span class="select-arrow">▾</span>
                        </div>
                      </div>

                      <div class="form-group">
                        <label class="form-label"
                          >Basis of Philippine Citizenship <span class="required">*</span></label
                        >
                        <div class="select-wrap">
                          <select v-model="appTypeForm.citizenshipBasis" class="form-select">
                            <option value="" disabled></option>
                            <option value="birth">By Birth</option>
                            <option value="election">By Election</option>
                            <option value="naturalization">By Naturalization</option>
                            <option value="reacquisition">By Re-acquisition</option>
                          </select>
                          <span class="select-arrow">▾</span>
                        </div>
                      </div>
                    </div>

                    <div class="form-group" style="margin-top: 18px">
                      <label class="form-label"
                        >Foreign Passport Holder <span class="required">*</span></label
                      >
                      <div class="toggle-group">
                        <button
                          class="toggle-btn"
                          :class="{ active: appTypeForm.foreignPassportHolder === true }"
                          @click="appTypeForm.foreignPassportHolder = true"
                        >
                          Yes
                        </button>
                        <button
                          class="toggle-btn"
                          :class="{ active: appTypeForm.foreignPassportHolder === false }"
                          @click="appTypeForm.foreignPassportHolder = false"
                        >
                          No
                        </button>
                      </div>
                    </div>

                    <div class="form-group" style="margin-top: 18px">
                      <label class="form-label"
                        >Courtesy Lane <span class="required">*</span></label
                      >
                      <div class="toggle-group">
                        <button
                          class="toggle-btn"
                          :class="{ active: appTypeForm.courtesyLane === true }"
                          @click="appTypeForm.courtesyLane = true"
                        >
                          Yes
                        </button>
                        <button
                          class="toggle-btn"
                          :class="{ active: appTypeForm.courtesyLane === false }"
                          @click="appTypeForm.courtesyLane = false"
                        >
                          No
                        </button>
                      </div>
                    </div>

                    <div class="form-group" style="margin-top: 18px">
                      <label class="form-label"
                        >Document Type <span class="required">*</span></label
                      >
                      <div class="select-wrap" style="max-width: 420px">
                        <select v-model="appTypeForm.documentType" class="form-select">
                          <option value="regular">Passport regular</option>
                          <option value="express">Passport express</option>
                          <option value="official">Official Passport</option>
                          <option value="diplomatic">Diplomatic Passport</option>
                        </select>
                        <span class="select-arrow">▾</span>
                      </div>
                    </div>

                    <div class="apptype-divider" />

                    <div class="apptype-row">
                      <div class="form-group">
                        <label class="form-label"
                          >Identification Number <span class="required">*</span></label
                        >
                        <input
                          v-model="appTypeForm.identificationNumber"
                          type="text"
                          class="form-input"
                          placeholder=""
                        />
                      </div>

                      <div class="form-group">
                        <label class="form-label"
                          >Identification Document Type <span class="required">*</span></label
                        >
                        <div class="select-wrap">
                          <select v-model="appTypeForm.identificationDocType" class="form-select">
                            <option value="" disabled></option>
                            <option value="birth-cert">PSA Birth Certificate</option>
                            <option value="voters-id">Voter's ID</option>
                            <option value="drivers-license">Driver's License</option>
                            <option value="philsys">PhilSys ID</option>
                            <option value="passport">Old Passport</option>
                          </select>
                          <span class="select-arrow">▾</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- ═══════════════════════════════════════════════════ -->
                <!-- TAB 2 — Companions & Assistants                    -->
                <!-- ═══════════════════════════════════════════════════ -->
                <div v-else-if="currentStep === 2">
                  <div class="ca-section-title">Assistant</div>
                  <div class="pds-section">
                    <div class="ca-notice">Assistants are optional for adults.</div>
                    <div class="ca-input-row">
                      <input
                        v-model="assistantInput"
                        type="text"
                        class="form-input"
                        placeholder="Assistant's Full Name"
                        @keyup.enter="addAssistant"
                      />
                      <button class="btn btn-ca-add" @click="addAssistant">Add</button>
                    </div>
                    <div v-if="assistants.length" class="ca-list">
                      <div v-for="(a, i) in assistants" :key="i" class="ca-list-item">
                        <span>{{ a }}</span>
                        <button class="ca-remove" @click="assistants.splice(i, 1)">✕</button>
                      </div>
                    </div>
                  </div>

                  <div class="ca-section-title">Companions</div>
                  <div class="pds-section">
                    <div class="ca-notice">Companions are optional for adults.</div>
                    <div class="ca-input-row">
                      <input
                        v-model="companionInput"
                        type="text"
                        class="form-input"
                        placeholder="Companion's Full Name"
                        @keyup.enter="addCompanion"
                      />
                      <button class="btn btn-ca-add" @click="addCompanion">Add</button>
                    </div>
                    <div v-if="companions.length" class="ca-list">
                      <div v-for="(c, i) in companions" :key="i" class="ca-list-item">
                        <span>{{ c }}</span>
                        <button class="ca-remove" @click="companions.splice(i, 1)">✕</button>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- ═══════════════════════════════════════════════════ -->
                <!-- TAB 3 — Application Form (review summary)          -->
                <!-- ═══════════════════════════════════════════════════ -->
                <div v-else-if="currentStep === 3">
                  <!-- Personal Information -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Personal Information</h3>
                    </div>
                    <div class="af-grid-3">
                      <div class="af-field">
                        <span class="af-label">First Name</span
                        ><span class="af-value">{{ appForm.firstName }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Middle Name</span
                        ><span class="af-value">{{ appForm.middleName }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Last Name</span
                        ><span class="af-value">{{ appForm.lastName }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Birth Date</span
                        ><span class="af-value">{{ appForm.birthDate }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Gender</span
                        ><span class="af-value">{{ appForm.gender }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Nationality</span
                        ><span class="af-value">{{ appForm.nationality }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Civil Status</span
                        ><span class="af-value">{{ appForm.civilStatus }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">PSA Birth Certificate</span
                        ><span class="af-value">{{ appForm.psaBirthCert }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Birth Legitimacy</span
                        ><span class="af-value">{{ appForm.birthLegitimacy }}</span>
                      </div>
                    </div>
                  </div>

                  <!-- Contact Information -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Contact Information</h3>
                    </div>
                    <div class="af-grid-3">
                      <div class="af-field af-span-2">
                        <span class="af-label">Current Address</span
                        ><span class="af-value">{{ appForm.currentAddress }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Current Country</span
                        ><span class="af-value">{{ appForm.currentCountry }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Current Region</span
                        ><span class="af-value">{{ appForm.currentRegion }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Current Barangay</span
                        ><span class="af-value">{{ appForm.currentBarangay }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Current Postal Code</span
                        ><span class="af-value">{{ appForm.postalCode }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Civil Status</span
                        ><span class="af-value">{{ appForm.civilStatus }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Personal Mobile</span
                        ><span class="af-value">{{ appForm.personalMobile }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Email</span
                        ><span class="af-value">{{ appForm.email }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Emergency Person</span
                        ><span class="af-value">{{ appForm.emergencyPerson }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Emergency Phone</span
                        ><span class="af-value">{{ appForm.emergencyPhone }}</span>
                      </div>
                    </div>
                  </div>

                  <!-- Family Members -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Family Members</h3>
                    </div>
                    <div class="af-family-grid">
                      <div class="af-family-col">
                        <div class="af-family-role">FATHER</div>
                        <div class="af-field">
                          <span class="af-label">First Name</span
                          ><span class="af-value">{{ appForm.fatherFirstName }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Last Name</span
                          ><span class="af-value">{{ appForm.fatherLastName }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Citizenship</span
                          ><span class="af-value">{{ appForm.fatherCitizenship }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Life Status</span
                          ><span class="af-value">{{ appForm.fatherLifeStatus }}</span>
                        </div>
                      </div>
                      <div class="af-family-col">
                        <div class="af-family-role">MOTHER</div>
                        <div class="af-field">
                          <span class="af-label">First Name</span
                          ><span class="af-value">{{ appForm.motherFirstName }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Last Name</span
                          ><span class="af-value">{{ appForm.motherLastName }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Citizenship</span
                          ><span class="af-value">{{ appForm.motherCitizenship }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Life Status</span
                          ><span class="af-value">{{ appForm.motherLifeStatus }}</span>
                        </div>
                      </div>
                      <div class="af-family-col">
                        <div class="af-family-role">SPOUSE</div>
                        <div class="af-field">
                          <span class="af-label">First Name</span
                          ><span class="af-value">{{ appForm.spouseFirstName }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Middle Name</span
                          ><span class="af-value">{{ appForm.spouseMiddleName }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Last Name</span
                          ><span class="af-value">{{ appForm.spouseLastName }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Citizenship</span
                          ><span class="af-value">{{ appForm.spouseCitizenship }}</span>
                        </div>
                        <div class="af-field">
                          <span class="af-label">Life Status</span
                          ><span class="af-value">{{ appForm.spouseLifeStatus }}</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Application Information -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Application Information</h3>
                    </div>
                    <div class="af-grid-3">
                      <div class="af-field">
                        <span class="af-label">Application Type</span
                        ><span class="af-value">{{ appTypeForm.applicationType || "—" }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Basis of Philippine Citizenship</span
                        ><span class="af-value">{{ appTypeForm.citizenshipBasis || "—" }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Courtesy Lane Type</span
                        ><span class="af-value">{{
                          appTypeForm.courtesyLane === true
                            ? "Yes"
                            : appTypeForm.courtesyLane === false
                              ? "No"
                              : "—"
                        }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Document Type</span
                        ><span class="af-value">{{ appTypeForm.documentType || "—" }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Identification Number</span
                        ><span class="af-value">{{ appTypeForm.identificationNumber || "—" }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Identification Document Type</span
                        ><span class="af-value">{{
                          appTypeForm.identificationDocType || "—"
                        }}</span>
                      </div>
                    </div>
                  </div>

                  <!-- Scheduled Date Information -->
                  <div class="pds-section">
                    <div class="pds-section-header">
                      <h3 class="pds-section-title">Scheduled Date Information</h3>
                    </div>
                    <div class="af-grid-3">
                      <div class="af-field">
                        <span class="af-label">Scheduled Date</span
                        ><span class="af-value">{{ selectedDate || "—" }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Scheduled Time</span
                        ><span class="af-value">{{ selectedTime || "—" }}</span>
                      </div>
                      <div class="af-field">
                        <span class="af-label">Site</span
                        ><span class="af-value">{{ selectedSiteInfo.name }}</span>
                      </div>
                    </div>
                  </div>

                  <!-- Declaration -->
                  <div class="af-declaration-box">
                    <label class="af-declaration-label">
                      <input
                        v-model="appForm.declarationChecked"
                        type="checkbox"
                        class="consent-checkbox"
                      />
                      <div class="af-declaration-text">
                        <p>
                          I am aware that possession of a passport is a privilege granted by the
                          Government of Republic of the Philippines only to its citizens.
                        </p>
                        <p>
                          I understand that any errors that I have committed in this online form may
                          result in my application being delayed and that any misrepresentation of
                          information on my part may be considered grounds for refusal/cancellation
                          of my application.
                        </p>
                        <p>
                          I consent to the use of my personal information provided herein for
                          Government to conduct the necessary records check and verification of
                          facts in connection with my passport application.
                        </p>
                        <p>
                          I hereby confirm that all information I have provided herein are true and
                          correct to the best of my knowledge.
                        </p>
                      </div>
                    </label>
                  </div>

                  <!-- Certify -->
                  <div class="af-certify-box">
                    <div class="af-certify-title">This is to certify that:</div>
                    <label class="af-certify-label">
                      <input
                        v-model="appForm.certifyChecked"
                        type="checkbox"
                        class="consent-checkbox"
                      />
                      <div class="af-declaration-text">
                        <p>The information entered above is true and correct.</p>
                        <p>I have the full knowledge in providing the above information.</p>
                        <p>
                          I understand the purpose of enrolling myself in the registry of the
                          Department of Foreign Affairs (DFA) ePassport Application System.
                        </p>
                        <p>
                          I have personally given my consent to allow the use of the information
                          contained in this form.
                        </p>
                        <p>
                          I understand that this form contains my personal information to be stored
                          in the DFA ePassport Database.
                        </p>
                      </div>
                    </label>
                    <p class="af-note">
                      <strong>Note:</strong> Please make sure all information displayed is correct
                      before proceeding.
                    </p>
                  </div>
                </div>

                <!-- ═══════════════════════════════════════════════════ -->
                <!-- TAB 4 — Documentary Requirements                   -->
                <!-- ═══════════════════════════════════════════════════ -->
                <div v-else-if="currentStep === 4">
                  <div class="pds-section">
                    <p class="appt-title">
                      Appointment for
                      <span class="hname">
                        {{
                          [appForm.firstName, appForm.middleName, appForm.lastName]
                            .filter(Boolean)
                            .join(" ")
                            .toUpperCase()
                        }}
                      </span>
                    </p>

                    <div class="file-notice">
                      File size limit is 5MB. Supported formats: jpeg, png, bmp, pdf
                    </div>

                    <p class="section-label">Core requirements<span class="required">*</span></p>

                    <div v-for="req in requirements" :key="req.id" class="req-item">
                      <label class="req-item__label"
                        >{{ req.label }}<span class="required">*</span></label
                      >
                      <div class="upload-zone">
                        <button class="browse-btn" @click.stop="triggerFileInput(req.id)">
                          + Browse
                        </button>
                      </div>
                      <div v-if="req.file" class="file-display">
                        <svg
                          width="14"
                          height="14"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="#888"
                          stroke-width="2"
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          style="flex-shrink: 0"
                        >
                          <path
                            d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"
                          />
                        </svg>
                        <span class="file-name">{{ req.file.name }}</span>
                        <button class="remove-btn" @click.stop="removeFile(req.id)">
                          <svg
                            width="14"
                            height="14"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2.5"
                          >
                            <line x1="18" y1="6" x2="6" y2="18" />
                            <line x1="6" y1="6" x2="18" y2="18" />
                          </svg>
                        </button>
                      </div>
                      <input
                        :ref="(el) => setFileInputRef(req.id, el)"
                        type="file"
                        accept=".jpeg,.jpg,.png,.bmp,.pdf"
                        class="hidden-input"
                        @change="onFileChange(req.id, $event)"
                      />
                      <hr class="divider" />
                    </div>
                  </div>

                  <!-- Conflict Detection Modal -->
                  <teleport to="body">
                    <div v-if="showConflictModal" class="modal-overlay" @click.self="closeModal">
                      <div class="modal-box">
                        <div class="modal-banner">
                          System found possible conflicts. Please confirm if the attachment is
                          valid.
                        </div>
                        <div class="modal-body">
                          <table class="conflict-table">
                            <thead>
                              <tr>
                                <th></th>
                                <th>Detected</th>
                                <th>Inserted</th>
                              </tr>
                            </thead>
                            <tbody>
                              <tr v-for="row in conflictRows" :key="row.field">
                                <td class="field-name">{{ row.field }}</td>
                                <td>{{ row.detected }}</td>
                                <td>{{ row.inserted }}</td>
                              </tr>
                            </tbody>
                          </table>
                          <div class="id-preview-wrap">
                            <img
                              v-if="pendingFilePreviewUrl"
                              :src="pendingFilePreviewUrl"
                              class="id-preview-img"
                              alt="Uploaded ID"
                            />
                            <div v-else class="id-preview-placeholder">Preview not available</div>
                          </div>
                        </div>
                        <div class="modal-actions">
                          <button class="btn-confirm" @click="confirmAttachment">
                            Confirm Attachment
                          </button>
                          <button class="btn-reupload" @click="uploadAgain">Upload again</button>
                        </div>
                      </div>
                    </div>
                  </teleport>
                </div>

                <!-- ═══════════════════════════════════════════════════ -->
                <!-- TAB 5 — Payment                                     -->
                <!-- ═══════════════════════════════════════════════════ -->
                <div v-else-if="currentStep === 5">
                  <div class="pds-section">
                    <p class="appt-title">
                      Appointment for <span class="hname">MARVIN ALFRED MERCADO PICO</span>
                    </p>

                    <!-- ─────────────────────────────────────
                         SELECTION VIEW (before I Agree)
                    ───────────────────────────────────── -->
                    <template v-if="!showPaymentStatus">
                      <!-- Processing Type -->
                      <div class="payment-group">
                        <p class="group-label">Choose a Processing Type</p>
                        <label
                          class="radio-card"
                          :class="{ selected: processingType === 'regular' }"
                        >
                          <input type="radio" value="regular" v-model="processingType" />
                          <div class="radio-card__content">
                            <p class="radio-card__title">Regular Processing (₱ 950.00)</p>
                            <p class="radio-card__sub">12 Working Days for All Consular Offices</p>
                          </div>
                        </label>
                        <label
                          class="radio-card"
                          :class="{ selected: processingType === 'special' }"
                        >
                          <input type="radio" value="special" v-model="processingType" />
                          <div class="radio-card__content">
                            <p class="radio-card__title">Special Processing (₱ 1,200.00)</p>
                            <p class="radio-card__sub">a. TOPS Processing, or</p>
                            <p class="radio-card__sub">
                              b. Expedite Processing (Applicable to DFA ASEANA And Consular Offices
                              Only)
                            </p>
                          </div>
                        </label>
                      </div>

                      <!-- Payment Method -->
                      <div class="payment-group">
                        <p class="group-label">Choose a Payment Method</p>
                        <label
                          class="radio-card"
                          :class="{ selected: paymentMethod === 'epayment' }"
                        >
                          <input type="radio" value="epayment" v-model="paymentMethod" />
                          <div class="radio-card__content">
                            <p class="radio-card__title">EPayment</p>
                            <p class="radio-card__sub">Pay via Maya/G Cash</p>
                          </div>
                        </label>
                        <label class="radio-card" :class="{ selected: paymentMethod === 'card' }">
                          <input type="radio" value="card" v-model="paymentMethod" />
                          <div class="radio-card__content">
                            <p class="radio-card__title">Debit/Credit Card</p>
                            <p class="radio-card__sub">Pay via Debit or Credit Card</p>
                          </div>
                        </label>
                        <label class="radio-card" :class="{ selected: paymentMethod === 'otc' }">
                          <input type="radio" value="otc" v-model="paymentMethod" />
                          <div class="radio-card__content">
                            <p class="radio-card__title">
                              Over the counter Payment to be made at authorized Payment Centers (₱
                              50.00 Convenience Fee)
                            </p>
                            <p class="radio-card__sub">Pay via Bayad Center accredited merchant</p>
                          </div>
                        </label>
                      </div>

                      <!-- Delivery Options -->
                      <div class="payment-group">
                        <p class="group-label">Delivery Options</p>
                        <label class="radio-card" :class="{ selected: deliveryOption === 'site' }">
                          <input type="radio" value="site" v-model="deliveryOption" />
                          <div class="radio-card__content">
                            <p class="radio-card__title">Site</p>
                            <p class="radio-card__sub">
                              DFA Manila (Aseana), ASEANA Business Park, Bradco Avenue corner
                              Macapagal Boulevard, Parañaque City
                            </p>
                          </div>
                        </label>
                        <label
                          class="radio-card"
                          :class="{ selected: deliveryOption === 'address' }"
                        >
                          <input type="radio" value="address" v-model="deliveryOption" />
                          <div class="radio-card__content">
                            <p class="radio-card__title">Current address</p>
                            <p class="radio-card__sub">
                              T1 U2124 LINEAR CONDOMINIUM, MALUGAY, SAN ANTONIO, MAKATI
                            </p>
                          </div>
                        </label>
                      </div>

                      <!-- Summary -->
                      <div
                        v-if="processingType && paymentMethod && deliveryOption"
                        class="summary-section"
                      >
                        <p class="group-label">Summary</p>
                        <table class="summary-table">
                          <thead>
                            <tr>
                              <th>Particulars</th>
                              <th>Quantity</th>
                              <th class="text-right">Amount</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr>
                              <td>Passport Fee</td>
                              <td>1</td>
                              <td class="text-right">
                                ₱ {{ processingType === "special" ? "1,200.00" : "950.00" }}
                              </td>
                            </tr>
                            <tr v-if="paymentMethod === 'otc'">
                              <td>Convenience Fee</td>
                              <td>1</td>
                              <td class="text-right">₱ 50.00</td>
                            </tr>
                            <tr class="total-row">
                              <td><strong>TOTAL</strong></td>
                              <td></td>
                              <td class="text-right">
                                <strong>₱ {{ totalAmount }}</strong>
                              </td>
                            </tr>
                          </tbody>
                        </table>
                        <div class="pay-actions">
                          <button class="btn-proceed-pay" @click="showPaymentModal = true">
                            Proceed to Payment
                          </button>
                        </div>
                      </div>
                    </template>

                    <!-- ─────────────────────────────────────
                         STATUS VIEW (after I Agree on modal)
                    ───────────────────────────────────── -->
                    <template v-else>
                      <p class="status-intro">This mode of payment is requesting for:</p>

                      <table class="status-table">
                        <tbody>
                          <tr>
                            <td class="st-label">Status</td>
                            <td class="st-value st-initiated">Initiated</td>
                          </tr>
                          <tr>
                            <td class="st-label">Amount</td>
                            <td class="st-value">₱ {{ totalAmount }}</td>
                          </tr>
                          <tr>
                            <td class="st-label">Total Amount Due</td>
                            <td class="st-value st-green">₱ {{ totalAmount }}</td>
                          </tr>
                        </tbody>
                      </table>

                      <div class="guidelines-title">General guidelines:</div>
                      <ul class="guidelines-list">
                        <li>
                          Pay in CASH at Bayad Center Branches and Authorized Partners (Robinsons
                          Malls &amp; Supermarkets, LBC, eBiz, Perahub, USSC, Truemoney, Villarica,
                          etc.), or through 7-11 and ECPay outlets. You may also pay via Bayad App
                          available in both iOS and Android devices.
                        </li>
                        <li>Confirmation Payments are processed once payed.</li>
                        <li>We will send a confirmation email to you once processed.</li>
                        <li>
                          Pay the exact amount. Any excess payment will be forfeited. Payments less
                          than the total amount indicated will not be processed.
                        </li>
                        <li>
                          Amount is inclusive of convenience fee. If you are paying for multiple
                          reference numbers, pay separately for each reference number. One
                          transaction per reference number.
                        </li>
                        <li>Make sure to get a reference number before paying.</li>
                        <li>
                          A reference number can only be used once. If you made a short payment by
                          mistake, do not try to correct it by making another bills payment with the
                          same reference number. Contact our helpdesk immediately.
                        </li>
                      </ul>

                      <p class="guidelines-link">
                        To check your appointment details, kindly proceed to this
                        <a href="#" class="link-blue">link</a>
                      </p>

                      <div class="status-nav">
                        <button class="btn-back-pay" @click="showPaymentStatus = false">
                          Back
                        </button>
                        <button class="btn-pay-now" @click="showPaymentSuccess = true">
                          Pay Now
                        </button>
                      </div>
                    </template>
                  </div>
                </div>
              </div>
            </transition>
          </div>

          <div class="nav-buttons">
            <button class="btn btn-back" :disabled="currentStep === 0" @click="prevStep">
              ← Back
            </button>
            <div class="step-indicator">Step {{ currentStep + 1 }} of {{ tabs.length }}</div>
            <button v-if="currentStep < tabs.length - 1" class="btn btn-next" @click="nextStep">
              Next →
            </button>
            <button
              v-else
              class="btn btn-submit"
              :disabled="!appForm.declarationChecked || !appForm.certifyChecked"
              @click="submit"
            >
              Submit Application
            </button>
          </div>
        </div>
      </template>
    </div>

    <!-- ═══════════════════════════════════════════════════════════ -->
    <!-- PAYMENT CONFIRMATION MODAL                                 -->
    <!-- ═══════════════════════════════════════════════════════════ -->
    <teleport to="body">
      <div v-if="showPaymentModal" class="modal-overlay" @click.self="showPaymentModal = false">
        <div class="modal-box payment-modal">
          <h3 class="modal-title">Payment Confirmation</h3>
          <div class="modal-scroll-body">
            <p class="modal-remind"><strong>Please be reminded of the following:</strong></p>
            <ul class="modal-list">
              <li>
                Your chosen mode of payment will charge a convenience fee in addition to the
                passport fee.
              </li>
              <li>
                Fees must be settled within <strong>24 hours</strong> upon receipt of the Reference
                Number.
              </li>
              <li>Your bank may not be available on weekends and holidays.</li>
              <li>
                Amount indicated is exclusively for the payment of the Passport Processing Fee.
                Processing fee is not refundable and not transferable.
              </li>
              <li>
                Applicants are advised to appear on their scheduled appointment with complete
                requirements or risk forfeiting payment. Applicants availing of courier service
                shall submit their current passport for cancellation during their appointment.
              </li>
              <li>
                <strong>For TOPS applicants:</strong> Passports for pick-up shall be claimed by you
                or your authorized representative at the designated Supervising Consular Office
                (SCO) of TOPS. There are no available courier delivery services on the TOPS site.
                Further, applicants at TOPS Sites will be unable to request the delivery of their
                passports on the day of their appointment schedule.
              </li>
            </ul>

            <p class="modal-remind"><strong>Basahin ang mga sumusunod na paalala:</strong></p>
            <ul class="modal-list">
              <li>
                Ang pinili ninyong mode of payment ay may karagdagang convenience fee, maliban sa
                passport fee.
              </li>
              <li>
                Ang mga fees ay kailangang mabayaran sa loob ng 24 oras matapos matanggap ang
                Reference Number. Ang mga bangko o bayad center ay maaaring hindi bukas ng weekend o
                holidays.
              </li>
              <li>
                Ang amount na nakasaad ay para lamang sa pagbabayad ng Passport Processing Fee.
              </li>
              <li>
                Ang processing fee ay hindi refundable at hindi maaaring i-transfer sa ibang
                aplikante.
              </li>
              <li>
                Kailangan po ninyong magpunta sa aming tanggapan sa araw ng inyong appointment, dala
                ang kumpletong passport requirements. Kung hindi makakapunta sa araw ng appointment,
                ang inyong bayad ay mawawalang bisa.
              </li>
              <li>
                Para sa mga nag-avail ng courier service, kailangang ipresenta ng mga aplikante ang
                lumang passport para makansela ito ng prosesor.
              </li>
              <li>
                Para sa mga aplikante sa TOPS: Ang mga passports na for pick-up ay kailangang kunin,
                ninyo o ng inyong representative sa designated na Supervising Consular Office (SCO)
                ng TOPS. Hindi maaaring mag-request sa TOPS na ipa-deliver ang inyong passport sa
                araw ng inyong appointment, dahil hindi available ang courier service sa mga TOPS
                Sites.
              </li>
            </ul>
          </div>
          <div class="modal-foot">
            <button class="btn-modal-cancel" @click="showPaymentModal = false">Cancel</button>
            <button class="btn-modal-agree" @click="agreePaymentModal">I Agree</button>
          </div>
        </div>
      </div>
    </teleport>

    <!-- ═══════════════════════════════════════════════════════════ -->
    <!-- PAYMENT SUCCESS MODAL                                      -->
    <!-- ═══════════════════════════════════════════════════════════ -->
    <teleport to="body">
      <div v-if="showPaymentSuccess" class="modal-overlay" @click.self="closePaymentSuccess">
        <div class="modal-box modal-small">
          <div class="success-icon-wrap">
            <svg width="56" height="56" viewBox="0 0 56 56" fill="none">
              <circle cx="28" cy="28" r="27" stroke="#34a853" stroke-width="2" />
              <polyline
                points="16,28 24,36 40,20"
                stroke="#34a853"
                stroke-width="2.5"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>
          </div>
          <h3 class="modal-title">Payment successful</h3>
          <p class="success-sub">Your payment has been successful!</p>
          <div class="modal-foot modal-foot--center">
            <button class="btn-modal-agree" @click="closePaymentSuccess">Close</button>
          </div>
        </div>
      </div>
    </teleport>

    <!-- ═══════════════════════════════════════════════════════════ -->
    <!-- ADD PROFILE MODAL                                          -->
    <!-- ═══════════════════════════════════════════════════════════ -->
    <teleport to="body">
      <div v-if="showAddProfileModal" class="modal-overlay" @click.self="closeAddProfileModal">
        <div class="modal-box" style="width: 560px">
          <h3 class="modal-title" style="padding: 18px 22px 0">Add Profile</h3>
          <p
            style="
              font-size: 12.5px;
              color: #718096;
              text-align: center;
              margin: 6px 0 0;
              padding: 0 22px;
            "
          >
            Fill in the details to create a new profile.
          </p>

          <div style="padding: 18px 22px; display: flex; flex-direction: column; gap: 14px">
            <!-- Row 1: Last Name, First Name, Middle Name, Suffix -->
            <div style="display: grid; grid-template-columns: 1fr 1fr 1fr 100px; gap: 12px">
              <div class="form-group">
                <label class="form-label">Last Name <span class="required">*</span></label>
                <input
                  v-model="newProfile.lastName"
                  type="text"
                  class="form-input"
                  placeholder="Last Name"
                />
              </div>
              <div class="form-group">
                <label class="form-label">First Name <span class="required">*</span></label>
                <input
                  v-model="newProfile.firstName"
                  type="text"
                  class="form-input"
                  placeholder="First Name"
                />
              </div>
              <div class="form-group">
                <label class="form-label">Middle Name</label>
                <input
                  v-model="newProfile.middleName"
                  type="text"
                  class="form-input"
                  placeholder="Middle Name"
                />
              </div>
              <div class="form-group">
                <label class="form-label">Suffix</label>
                <div class="select-wrap">
                  <select v-model="newProfile.suffix" class="form-select">
                    <option value="">—</option>
                    <option value="Jr.">Jr.</option>
                    <option value="Sr.">Sr.</option>
                    <option value="II">II</option>
                    <option value="III">III</option>
                    <option value="IV">IV</option>
                  </select>
                  <span class="select-arrow">▾</span>
                </div>
              </div>
            </div>

            <!-- Row 2: Relationship -->
            <div class="form-group">
              <label class="form-label">Relationship <span class="required">*</span></label>
              <div class="select-wrap">
                <select v-model="newProfile.relationship" class="form-select">
                  <option value="" disabled>Select relationship</option>
                  <option value="Father">Father</option>
                  <option value="Mother">Mother</option>
                  <option value="Spouse">Spouse</option>
                  <option value="Sibling">Sibling</option>
                  <option value="Child">Child</option>
                  <option value="Guardian">Guardian</option>
                  <option value="Self">Self</option>
                </select>
                <span class="select-arrow">▾</span>
              </div>
            </div>

            <!-- Error -->
            <p v-if="addProfileError" style="font-size: 0.78rem; color: #e53e3e; margin: 0">
              {{ addProfileError }}
            </p>
          </div>

          <div class="modal-foot">
            <button class="btn-modal-cancel" @click="closeAddProfileModal">Cancel</button>
            <button class="btn-modal-agree" @click="submitAddProfile">Add Profile</button>
          </div>
        </div>
      </div>
    </teleport>

    <!-- ═══════════════════════════════════════════════════════════ -->
    <!-- DFA ePAYMENT RECEIPT MODAL                                 -->
    <!-- ═══════════════════════════════════════════════════════════ -->
    <teleport to="body">
      <div v-if="showEReceipt" class="modal-overlay" @click.self="showEReceipt = false">
        <div class="modal-box modal-small">
          <!-- DFA header — no logo as requested -->
          <div class="dfa-header">
            <div>
              <p class="dfa-dept">Department of Foreign Affairs</p>
              <p class="dfa-epay">ePayment Services</p>
            </div>
          </div>

          <p class="epay-intro">This mode of payment is requesting for...</p>

          <table class="epay-table">
            <tbody>
              <tr>
                <td class="ep-label">Status</td>
                <td class="ep-value ep-bold">PAID</td>
              </tr>
              <tr>
                <td class="ep-label">Scheduled Date</td>
                <td class="ep-value">{{ formatSelectedSchedule || "9 January 2025, 09:00" }}</td>
              </tr>
              <tr>
                <td class="ep-label">Amount</td>
                <td class="ep-value">PHP {{ totalAmount }}</td>
              </tr>
              <tr>
                <td class="ep-label">Total Amount Due</td>
                <td class="ep-value ep-green">PHP {{ totalAmount }}</td>
              </tr>
            </tbody>
          </table>

          <p class="not-receipt">This is not your e-Receipt.</p>

          <div class="modal-foot modal-foot--center">
            <button class="btn-modal-agree" @click="showEReceipt = false">Close</button>
          </div>
        </div>
      </div>
    </teleport>
  </div>
</template>

<script setup>
import LeftMenu from "@/components/LeftMenu.vue";
import { ref, computed, onMounted } from "vue";
import axios from "axios";
import { useAuthStore } from "../stores/auth";
import { BACKEND_DOMAIN } from "@/configs/config";

const Auth = useAuthStore();

// ── Pre-step state ──────────────────────────────────────────────────
const preStep = ref("terms");
const consentChecked = ref(false);

// ── Pre-step handlers ───────────────────────────────────────────────
const continueExisting = () => {};

const startIndividual = () => {
  if (!consentChecked.value) return;
  preStep.value = "selectProfile";
};

const startGroup = () => {
  if (!consentChecked.value) return;
  preStep.value = "selectProfile";
};

const cancelNotice = () => {
  preStep.value = "terms";
};

const agreeNotice = () => {
  preStep.value = null;
};

// ── Main stepper ────────────────────────────────────────────────────
const tabs = [
  "Site Location & Schedule",
  "Application Type",
  "Companions & Assistants",
  "Application Form",
  "Documentary Requirements",
  "Payment",
];

const currentStep = ref(0);

const nextStep = () => {
  if (currentStep.value < tabs.length - 1) currentStep.value++;
};

const prevStep = () => {
  if (currentStep.value > 0) currentStep.value--;
};

// ── Tab 0: Site Location & Schedule ────────────────────────────────
const siteForm = ref({
  region: "asia-pacific",
  country: "philippines",
  site: "dfa-aseana",
});

const siteInfoMap = {
  "dfa-aseana": {
    name: "DFA Manila (ASEANA)",
    address: "ASEANA Business Park, Bradco Avenue corner Macapagal Boulevard, Parañaque City",
    contact: "556-0000 / 651-9400",
  },
  "dfa-san-fernando": {
    name: "DFA San Fernando",
    address: "2/F Robinsons Place Pampanga, San Fernando, Pampanga",
    contact: "(045) 455-1400",
  },
  "dfa-cebu": {
    name: "DFA Cebu",
    address: "Superblock, Maguikay, Mandaue City, Cebu",
    contact: "(032) 232-3365",
  },
  "dfa-davao": {
    name: "DFA Davao",
    address: "Km. 6, Lanang, Davao City",
    contact: "(082) 234-2357",
  },
};

const selectedSiteInfo = computed(
  () => siteInfoMap[siteForm.value.site] ?? siteInfoMap["dfa-aseana"],
);

// Calendar
const calViewYear = ref(new Date().getFullYear());
const calViewMonth = ref(new Date().getMonth());

const MONTH_NAMES = [
  "January",
  "February",
  "March",
  "April",
  "May",
  "June",
  "July",
  "August",
  "September",
  "October",
  "November",
  "December",
];

const calMonthLabel = computed(() => `${MONTH_NAMES[calViewMonth.value]}   ${calViewYear.value}`);

const prevMonth = () => {
  if (calViewMonth.value === 0) {
    calViewMonth.value = 11;
    calViewYear.value--;
  } else calViewMonth.value--;
};
const nextMonth = () => {
  if (calViewMonth.value === 11) {
    calViewMonth.value = 0;
    calViewYear.value++;
  } else calViewMonth.value++;
};

const calStartBlank = computed(() => {
  return new Date(calViewYear.value, calViewMonth.value, 1).getDay();
});

const calDaysInMonth = computed(() => {
  const year = calViewYear.value;
  const month = calViewMonth.value;
  const total = new Date(year, month + 1, 0).getDate();
  const today = new Date();
  const todayStr = `${today.getFullYear()}-${today.getMonth()}-${today.getDate()}`;
  const days = [];
  for (let d = 1; d <= total; d++) {
    const dateStr = `${year}-${month}-${d}`;
    const dayOfWeek = new Date(year, month, d).getDay();
    const isPast =
      new Date(year, month, d) < new Date(today.getFullYear(), today.getMonth(), today.getDate());
    const isWeekend = dayOfWeek === 0;
    days.push({
      num: d,
      date: dateStr,
      available: !isPast && !isWeekend,
      isToday: dateStr === todayStr,
    });
  }
  return days;
});

const getEarliestAvailableDate = () => {
  const year = calViewYear.value;
  const month = calViewMonth.value;
  const today = new Date();
  const total = new Date(year, month + 1, 0).getDate();
  for (let d = 1; d <= total; d++) {
    const dayOfWeek = new Date(year, month, d).getDay();
    const isPast =
      new Date(year, month, d) < new Date(today.getFullYear(), today.getMonth(), today.getDate());
    if (!isPast && dayOfWeek !== 0) {
      return `${year}-${String(month + 1).padStart(2, "0")}-${String(d).padStart(2, "0")}`;
    }
  }
  return null;
};

const selectedDate = ref(getEarliestAvailableDate());
const selectedTime = ref("9:00 AM - 9:30 AM");

const selectDate = (date) => {
  selectedDate.value = date;
};

const earliestAppointmentLabel = computed(() => {
  const avail = calDaysInMonth.value.find((d) => d.available);
  if (!avail) return "N/A";
  return `${MONTH_NAMES[calViewMonth.value]} ${avail.num}, ${calViewYear.value}`;
});

const formatSelectedSchedule = computed(() => {
  if (!selectedDate.value || !selectedTime.value) return "—";
  const [year, month, day] = selectedDate.value.split("-");
  return `${parseInt(day)} ${MONTH_NAMES[parseInt(month)]} ${year} | ${selectedTime.value}`;
});

const timeSlots = ref([
  { time: "9:00 AM - 9:30 AM", slots: 8 },
  { time: "9:30 AM - 10:00 AM", slots: 8 },
  { time: "10:00 AM - 10:30 AM", slots: 8 },
  { time: "10:30 AM - 11:00 AM", slots: 8 },
  { time: "11:00 AM - 11:30 AM", slots: 8 },
  { time: "11:30 AM - 12:00 PM", slots: 8 },
  { time: "1:00 PM - 1:30 PM", slots: 8 },
  { time: "1:30 PM - 2:00 PM", slots: 0 },
  { time: "2:00 PM - 2:30 PM", slots: 8 },
]);

// ── Tab 1: Application Type ─────────────────────────────────────────
const appTypeForm = ref({
  applicationType: "",
  citizenshipBasis: "",
  foreignPassportHolder: null,
  courtesyLane: null,
  documentType: "regular",
  identificationNumber: "",
  identificationDocType: "",
});

// ── Tab 2: Companions & Assistants ─────────────────────────────────
const assistantInput = ref("");
const assistants = ref([]);
const companionInput = ref("");
const companions = ref([]);

const addAssistant = () => {
  const name = assistantInput.value.trim();
  if (name) {
    assistants.value.push(name);
    assistantInput.value = "";
  }
};
const addCompanion = () => {
  const name = companionInput.value.trim();
  if (name) {
    companions.value.push(name);
    companionInput.value = "";
  }
};

// ── Tab 3: Application Form ─────────────────────────────────────────
const appForm = ref({
  // Personal
  firstName: "",
  middleName: "",
  lastName: "",
  birthDate: "",
  gender: "",
  nationality: "",
  civilStatus: "",
  psaBirthCert: "",
  birthLegitimacy: "",

  // Contact
  currentAddress: "",
  currentCountry: "",
  currentRegion: "",
  currentBarangay: "",
  postalCode: "",
  personalMobile: "",
  email: "",
  emergencyPerson: "",
  emergencyPhone: "",

  // Father
  fatherFirstName: "",
  fatherLastName: "",
  fatherCitizenship: "",
  fatherLifeStatus: "",

  // Mother
  motherFirstName: "",
  motherLastName: "",
  motherCitizenship: "",
  motherLifeStatus: "",

  // Spouse
  spouseFirstName: "",
  spouseMiddleName: "",
  spouseLastName: "",
  spouseCitizenship: "",
  spouseLifeStatus: "",

  // Checkboxes
  declarationChecked: false,
  certifyChecked: false,
});

// ── Tab 4: Documentary Requirements ────────────────────────────────
const MAX_FILE_SIZE = 5 * 1024 * 1024;
const SUPPORTED_TYPES = ["image/jpeg", "image/jpg", "image/png", "image/bmp", "application/pdf"];
const CONFLICT_IDS = ["gov-id"];

const requirements = ref([
  { id: "gov-id", label: "Valid Government Issued ID", file: null },
  { id: "birth-cert", label: "PSA Birth Certificate or Local Civil Registrar Copy", file: null },
]);

const fileInputs = ref({});
const showConflictModal = ref(false);
const pendingFile = ref(null);
const pendingFilePreviewUrl = ref(null);

const detectedData = ref({
  crn: "0028-1215160-9",
  surname: "SANTOS",
  givenName: "JOSE",
  middleName: "CRUZ",
  sex: "M",
  dob: "1960/01/28",
  addressLine1: "28 PAYAPA ST BAGONG DIWA",
  addressLine2: "STO CRISTOBAL CALOOCAN CITY",
  addressLine3: "METRO MANILA PHILIPPINES 1800",
});

const conflictRows = computed(() => [
  {
    field: "Full Name",
    detected: `${detectedData.value.givenName} ${detectedData.value.middleName} ${detectedData.value.surname}`,
    inserted: `${appForm.value.firstName} ${appForm.value.middleName} ${appForm.value.lastName}`,
  },
  {
    field: "Address",
    detected: [
      detectedData.value.addressLine1,
      detectedData.value.addressLine2,
      detectedData.value.addressLine3,
    ].join(" "),
    inserted: appForm.value.currentAddress,
  },
  { field: "Date Of Birth", detected: "1960-01-28", inserted: appForm.value.birthDate },
  { field: "Gender", detected: detectedData.value.sex, inserted: appForm.value.gender },
  { field: "Place Of Birth", detected: "", inserted: "" },
  { field: "Occupation", detected: "", inserted: "" },
  { field: "Nationality", detected: "", inserted: appForm.value.nationality },
]);

const setFileInputRef = (id, el) => {
  if (el) fileInputs.value[id] = el;
};
const triggerFileInput = (id) => {
  fileInputs.value[id]?.click();
};

const validateFile = (file) => {
  if (file.size > MAX_FILE_SIZE) {
    alert(`File exceeds 5MB.`);
    return false;
  }
  if (!SUPPORTED_TYPES.includes(file.type)) {
    alert("Unsupported format. Use jpeg, jpg, png, bmp, or pdf.");
    return false;
  }
  return true;
};

const onFileChange = (id, event) => {
  const file = event.target.files[0];
  if (!file) return;
  if (!validateFile(file)) {
    event.target.value = "";
    return;
  }

  if (CONFLICT_IDS.includes(id)) {
    pendingFile.value = { id, file };
    const reader = new FileReader();
    reader.onload = (e) => {
      pendingFilePreviewUrl.value = e.target.result;
      showConflictModal.value = true;
    };
    reader.readAsDataURL(file);
  } else {
    attachFile(id, file);
  }
  event.target.value = "";
};

const removeFile = (id) => {
  const req = requirements.value.find((r) => r.id === id);
  if (req) req.file = null;
};

const confirmAttachment = () => {
  if (pendingFile.value) {
    attachFile(pendingFile.value.id, pendingFile.value.file);
    pendingFile.value = null;
  }
  pendingFilePreviewUrl.value = null;
  showConflictModal.value = false;
};

const uploadAgain = () => {
  pendingFile.value = null;
  pendingFilePreviewUrl.value = null;
  showConflictModal.value = false;
  setTimeout(() => fileInputs.value["gov-id"]?.click(), 100);
};

const closeModal = () => {
  pendingFile.value = null;
  pendingFilePreviewUrl.value = null;
  showConflictModal.value = false;
};

const attachFile = (id, file) => {
  const req = requirements.value.find((r) => r.id === id);
  if (req) req.file = file;
};

// ── Tab 5: Payment ──────────────────────────────────────────────────
const processingType = ref(null);
const paymentMethod = ref(null);
const deliveryOption = ref(null);

// Modal visibility
const showPaymentModal = ref(false);
const showPaymentStatus = ref(false); // controls selection vs status panel inside step 5
const showPaymentSuccess = ref(false);
const showEReceipt = ref(false);

const totalAmount = computed(() => {
  let base = processingType.value === "special" ? 1200 : 950;
  if (paymentMethod.value === "otc") base += 50;
  return base.toLocaleString("en-PH", { minimumFractionDigits: 2 });
});

// Step 1: User clicks "I Agree" on Payment Confirmation modal
//         → hide modal, show Status panel inside step 5
const agreePaymentModal = () => {
  showPaymentModal.value = false;
  showPaymentStatus.value = true;
};

// Step 2: User clicks "Pay Now" on Status panel → show Success modal
// (handled inline via @click="showPaymentSuccess = true")

// Step 3: User clicks "Close" on Success modal → show eReceipt modal
const closePaymentSuccess = () => {
  showPaymentSuccess.value = false;
  showEReceipt.value = true;
};

// GET PROFILES LIST
// Reactive states
const profiles = ref([]);
const selectedProfile = ref(null);
const loading = ref(false);
const error = ref(null);

// Fetch profiles from backend
const fetchProfiles = async () => {
  loading.value = true;
  error.value = null;
  try {
    const res = await axios.get(`${BACKEND_DOMAIN}/api/PassportProfile/Profiles`, {
      headers: {
        Authorization: `Bearer ${Auth.token}`,
      },
    });

    // Use backend-provided fullName and relationship
    profiles.value = res.data.map((p) => ({
      id: p.passportPersonalInformationId,
      name: p.fullName,
      relationship: p.relationship ?? "Personal",
    }));
  } catch (err) {
    console.error(err);
    error.value = "Failed to load profiles.";
  } finally {
    loading.value = false;
  }
};

// Trigger API call on mount
onMounted(() => {
  fetchProfiles();
});

// Handle proceed action
const proceedWithProfile = async () => {
  if (!selectedProfile.value) return;

  try {
    const res = await axios.get(`${BACKEND_DOMAIN}/api/PassportProfile/${selectedProfile.value}`, {
      headers: { Authorization: `Bearer ${Auth.token}` },
    });

    const { personal, family, contact } = res.data;

    // ── Find family members by relationship ──────────────
    const father = family?.find((f) => f.relationship === "Father");
    const mother = family?.find((f) => f.relationship === "Mother");
    const spouse = family?.find((f) => f.relationship === "Spouse");

    // ── Map API response → appForm ───────────────────────
    appForm.value = {
      ...appForm.value, // keep declarationChecked / certifyChecked as false

      // Personal
      firstName: personal.firstName ?? "",
      middleName: personal.middleName ?? "",
      lastName: personal.lastName ?? "",
      birthDate: personal.birthdate
        ? new Date(personal.birthdate).toLocaleDateString("en-US", {
            month: "short",
            day: "numeric",
            year: "numeric",
          })
        : "",
      gender: personal.gender ?? "",
      nationality: personal.nationality ?? "",
      civilStatus: personal.civilStatusId ?? "",
      psaBirthCert: personal.hasPSABirthcert ? "Yes" : "No",
      birthLegitimacy: personal.birthLegitimacy ?? "",

      // Contact
      currentAddress: [
        contact?.currentBarangay,
        contact?.currentCityMunicipality,
        contact?.currentProvince,
        contact?.currentRegion,
      ]
        .filter(Boolean)
        .join(", "),
      currentCountry: personal.birthCountry ?? "",
      currentRegion: contact?.currentRegion ?? "",
      currentBarangay: contact?.currentBarangay ?? "",
      postalCode: contact?.currentPostalCode ?? "",
      personalMobile: contact?.personalMobileNumber ?? "",
      email: contact?.email ?? "",

      // Emergency — not in API yet, keep existing or blank
      emergencyPerson: appForm.value.emergencyPerson,
      emergencyPhone: appForm.value.emergencyPhone,

      // Father
      fatherFirstName: father?.firstName ?? "",
      fatherLastName: father?.lastName ?? "",
      fatherCitizenship: father?.citizenship ?? "",
      fatherLifeStatus: father?.isAlive ? "Alive" : "Deceased",

      // Mother
      motherFirstName: mother?.firstName ?? "",
      motherLastName: mother?.lastName ?? "",
      motherCitizenship: mother?.citizenship ?? "",
      motherLifeStatus: mother?.isAlive ? "Alive" : "Deceased",

      // Spouse
      spouseFirstName: spouse?.firstName ?? "",
      spouseMiddleName: spouse?.middleName ?? "",
      spouseLastName: spouse?.lastName ?? "",
      spouseCitizenship: spouse?.citizenship ?? "",
      spouseLifeStatus: spouse?.isAlive ? "Alive" : "Deceased",

      // Keep checkboxes untouched
      declarationChecked: false,
      certifyChecked: false,
    };

    preStep.value = "appointmentNotice";
  } catch (err) {
    console.error("Failed to fetch profile:", err);
    alert("Failed to load profile data. Please try again.");
  }
};

const submit = async () => {
  try {
    // ── Debug logs FIRST ────────────────────────────────
    console.log("selectedDate:", selectedDate.value);
    console.log("selectedTime:", selectedTime.value);
    console.log("selectedProfile:", selectedProfile.value);
    console.log("appTypeForm:", appTypeForm.value);
    console.log("processingType:", processingType.value);
    console.log("paymentMethod:", paymentMethod.value);
    console.log("deliveryOption:", deliveryOption.value);

    const formData = new FormData();

    // Profile
    formData.append("PassportPersonalInformationId", selectedProfile.value);

    // Site & Schedule
    formData.append("Region", siteForm.value.region);
    formData.append("Country", siteForm.value.country);
    formData.append("Site", siteForm.value.site);

    // ── Fix: safely parse the date ──────────────────────
    // selectedDate is "2026-3-29", pad it to "2026-03-29"
    const [y, m, d] = selectedDate.value.split("-");
    const paddedDate = `${y}-${String(m).padStart(2, "0")}-${String(d).padStart(2, "0")}`;

    // selectedTime is "9:00 AM - 9:30 AM", take the start
    const timeStart = selectedTime.value?.split(" - ")[0]?.trim() ?? "09:00 AM";

    // Convert "9:00 AM" → 24h for reliable parsing
    const [timePart, meridiem] = timeStart.split(" ");
    let [hours, minutes] = timePart.split(":").map(Number);
    if (meridiem === "PM" && hours !== 12) hours += 12;
    if (meridiem === "AM" && hours === 12) hours = 0;
    const paddedTime = `${String(hours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}:00`;

    const scheduleDate = new Date(`${paddedDate}T${paddedTime}`);
    console.log("Parsed scheduleDate:", scheduleDate.toISOString());

    formData.append("Schedule", scheduleDate.toISOString());

    // Application Type
    formData.append("ApplicationType", appTypeForm.value.applicationType);
    formData.append("CitizenshipBasis", appTypeForm.value.citizenshipBasis);
    formData.append("isForeignPassportHolder", appTypeForm.value.foreignPassportHolder ?? false);
    formData.append("isCourtesyLane", appTypeForm.value.courtesyLane ?? false);
    formData.append("DocumentType", appTypeForm.value.documentType);
    formData.append("IdDocumentIdNumber", appTypeForm.value.identificationNumber);
    formData.append("IdDocumentType", appTypeForm.value.identificationDocType);

    // Files
    const validIdReq = requirements.value.find((r) => r.id === "gov-id");
    const certReq = requirements.value.find((r) => r.id === "birth-cert");
    console.log("ValidId file:", validIdReq?.file);
    console.log("Certificate file:", certReq?.file);

    if (!validIdReq?.file || !certReq?.file) {
      alert("Please upload all required documents before submitting.");
      return;
    }
    formData.append("ValidId", validIdReq.file);
    formData.append("Certificate", certReq.file);

    // Payment
    formData.append("ProcessingType", processingType.value ?? "");
    formData.append("PaymentMethod", paymentMethod.value ?? "");
    formData.append("DeliveryOption", deliveryOption.value ?? "");
    formData.append("isPaid", showPaymentSuccess.value);
   formData.append("Amount", totalAmount.value.replace(/,/g, ""));

    // ── Log full FormData before sending ────────────────
    for (let [key, val] of formData.entries()) {
      console.log("FormData →", key, val);
    }

    const res = await axios.post(`${BACKEND_DOMAIN}/api/Application`, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
        Authorization: `Bearer ${Auth.token}`,
      },
    });

    alert("Application submitted successfully!");
    console.log("Response:", res.data);
  } catch (err) {
    // ── This will now catch JS errors AND axios 4xx/5xx ─
    if (err?.response) {
      console.error("HTTP error status:", err.response.status);
      console.error("HTTP error data:", err.response.data);
      console.error("Validation errors:", JSON.stringify(err.response.data?.errors, null, 2));

      const errors = err.response.data?.errors;
      if (errors) {
        const msg = Object.entries(errors)
          .map(([field, messages]) => `${field}: ${messages.join(", ")}`)
          .join("\n");
        alert(`Validation errors:\n${msg}`);
      } else {
        alert(err.response.data?.message ?? err.response.data?.title ?? "Submission failed.");
      }
    } else {
      // JS runtime error (e.g. Invalid Date)
      console.error("Runtime error:", err.message, err.stack);
      alert(`Runtime error: ${err.message}`);
    }
  }
};
</script>

<style scoped>
/* ── Layout ──────────────────────────────────────────────────────── */
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
  padding: 30px;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.page-title {
  font-size: 1.8rem;
  color: #06195e;
  font-weight: 800;
  margin-bottom: 20px;
}

/* ── Pre-step wrapper ────────────────────────────────────────────── */
.pre-step-wrapper {
  flex: 1;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  overflow-y: auto;
}

/* ── Terms card ──────────────────────────────────────────────────── */
.terms-card {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  padding: 36px 40px;
  max-width: 680px;
  width: 100%;
  box-shadow: 0 4px 16px rgba(6, 25, 94, 0.07);
}

.terms-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1a202c;
  text-align: center;
  margin: 0 0 6px;
  line-height: 1.5;
}

.terms-title strong {
  font-weight: 800;
}

.terms-subtitle {
  font-size: 0.82rem;
  color: #718096;
  text-align: center;
  margin: 0 0 20px;
}

.reminder-box {
  background: #fff8f0;
  border: 1px solid #f6ad55;
  border-radius: 8px;
  padding: 12px 16px;
  margin-bottom: 18px;
}

.reminder-label {
  font-size: 0.8rem;
  font-weight: 700;
  color: #c05621;
  display: block;
  margin-bottom: 4px;
}

.reminder-box p {
  margin: 0;
  font-size: 0.82rem;
  color: #744210;
}

.terms-section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #1a202c;
  margin: 18px 0 8px;
}

.terms-card p {
  font-size: 0.82rem;
  color: #4a5568;
  line-height: 1.6;
  margin: 0 0 10px;
}

.consent-row {
  background: #f0f4ff;
  border: 1.5px solid #a3b4e8;
  border-radius: 8px;
  padding: 14px 16px;
  margin: 18px 0 22px;
}

.consent-label {
  display: flex;
  gap: 10px;
  cursor: pointer;
}

.consent-checkbox {
  flex-shrink: 0;
  width: 16px;
  height: 16px;
  margin-top: 2px;
  accent-color: #06195e;
  cursor: pointer;
}

.consent-text {
  font-size: 0.78rem;
  color: #2b4b9e;
  line-height: 1.6;
}

.terms-actions {
  display: flex;
  flex-direction: column;
  gap: 14px;
  align-items: center;
}

.terms-new-actions {
  display: flex;
  gap: 16px;
  align-items: flex-start;
  justify-content: center;
  flex-wrap: wrap;
}

.group-btn-wrap {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.group-note {
  font-size: 0.72rem;
  color: #718096;
  margin: 4px 0 0;
  text-align: center;
}

/* ── Modal overlay ───────────────────────────────────────────────── */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(30, 40, 80, 0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 200;
  backdrop-filter: blur(2px);
}

.modal-card {
  background: #fff;
  border-radius: 14px;
  padding: 32px 36px;
  width: 420px;
  max-width: 92vw;
  box-shadow: 0 8px 40px rgba(6, 25, 94, 0.18);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.notice-card {
  width: 520px;
  align-items: flex-start;
}

.modal-icon {
  margin-bottom: 6px;
}

.modal-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #06195e;
  margin: 0;
  text-align: center;
}

.notice-card .modal-title {
  text-align: left;
}

.modal-subtitle {
  font-size: 0.82rem;
  color: #718096;
  text-align: center;
  margin: 0 0 16px;
}

.notice-card .modal-subtitle {
  text-align: left;
}

/* profile list */
.profile-list {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 18px;
}

.profile-option {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  cursor: pointer;
  transition:
    border-color 0.15s,
    background 0.15s;
}

.profile-option.selected {
  border-color: #06195e;
  background: #f0f4ff;
}

.profile-radio-input {
  display: none;
}

.profile-icon {
  flex-shrink: 0;
}

.profile-name {
  flex: 1;
  font-size: 0.87rem;
  font-weight: 600;
  color: #1a202c;
}

.profile-radio {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: 2px solid #06195e;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.radio-dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: #06195e;
}

/* notice list */
.notice-list {
  padding-left: 20px;
  margin: 0 0 20px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.notice-list li {
  font-size: 0.82rem;
  color: #4a5568;
  line-height: 1.6;
}

.notice-link {
  color: #2b4b9e;
  text-decoration: underline;
}

.notice-actions {
  display: flex;
  gap: 12px;
  align-self: flex-end;
}

/* ── Buttons ─────────────────────────────────────────────────────── */
.btn {
  padding: 10px 24px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.9rem;
  cursor: pointer;
  border: none;
  transition: all 0.2s;
}

.btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.btn-continue-existing {
  background: #e67e22;
  color: #fff;
}

.btn-continue-existing:hover {
  background: #cf6d17;
}

.btn-individual {
  background: #06195e;
  color: #fff;
}

.btn-individual:not(:disabled):hover {
  background: #04134a;
}

.btn-group {
  background: #e2e8f0;
  color: #4a5568;
  border: 1.5px solid #cbd5e0;
}

.btn-group:not(:disabled):hover {
  background: #cbd5e0;
}

.btn-proceed {
  background: #06195e;
  color: #fff;
  width: 100%;
  margin-top: 4px;
  padding: 12px;
}

.btn-proceed:not(:disabled):hover {
  background: #04134a;
}

.btn-proceed:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.btn-cancel {
  background: #f1f5f9;
  color: #4a5568;
  border: 1.5px solid #d1d9e6;
}

.btn-cancel:hover {
  background: #e2e8f0;
}

.btn-agree {
  background: #06195e;
  color: #fff;
}

.btn-agree:hover {
  background: #04134a;
}

/* ── Stepper ─────────────────────────────────────────────────────── */
.stepper {
  display: flex;
  align-items: flex-start;
  padding: 16px 0 0;
  overflow-x: auto;
}

.step {
  display: flex;
  flex-direction: column;
  align-items: center;
  flex: 1;
  min-width: 80px;
  position: relative;
}

.step:not(:last-child)::after {
  content: "";
  position: absolute;
  top: 18px;
  left: calc(50% + 18px);
  right: calc(-50% + 18px);
  height: 2px;
  background: #d1d9e6;
  z-index: 0;
}

.step.done:not(:last-child)::after,
.step.active:not(:last-child)::after {
  background: #06195e;
}

.step-circle {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 600;
  border: 2px solid #d1d9e6;
  background: #f4f7f9;
  color: #718096;
  position: relative;
  z-index: 1;
  transition: all 0.2s;
}

.step.active .step-circle {
  border-color: #06195e;
  background: #eef2fb;
  color: #06195e;
}

.step.done .step-circle {
  border-color: #06195e;
  background: #06195e;
  color: #fff;
}

.step-label {
  margin-top: 8px;
  font-size: 11px;
  color: #718096;
  text-align: center;
  line-height: 1.4;
  max-width: 90px;
}

.step.active .step-label {
  color: #06195e;
  font-weight: 600;
}

.step.done .step-label {
  color: #06195e;
}

/* ── Tab wrapper ─────────────────────────────────────────────────── */
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
  margin-top: 0;
}

.tab-content {
  flex: 1;
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
}

.pds-section-header {
  margin-bottom: 12px;
  padding-bottom: 12px;
  border-bottom: 2px solid #f0f4f9;
}

.pds-section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #06195e;
  margin: 0;
}

.nav-buttons {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px solid #e2e8f0;
}

.step-indicator {
  font-size: 0.82rem;
  color: #a0aec0;
  font-weight: 500;
}

.btn-back {
  background: #f1f5f9;
  color: #4a5568;
  border: 1.5px solid #d1d9e6;
}

.btn-back:not(:disabled):hover {
  background: #e2e8f0;
}

.btn-next {
  background: #06195e;
  color: #fff;
}

.btn-next:hover {
  background: #04134a;
}

.btn-submit {
  background: #276749;
  color: #fff;
}

.btn-submit:hover {
  background: #1e5038;
}

/* ── Transitions ─────────────────────────────────────────────────── */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition:
    opacity 0.25s ease,
    transform 0.25s ease;
}

.fade-slide-enter-from {
  opacity: 0;
  transform: translateX(16px);
}

.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-16px);
}

/* ── Tab 0: Site Location & Schedule ────────────────────────────── */
.site-location-title {
  color: #2b4b9e !important;
}

.site-location-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin-top: 4px;
}

.site-dropdowns {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.form-label {
  font-size: 0.82rem;
  font-weight: 600;
  color: #4a5568;
}

.required {
  color: #e53e3e;
}

.select-wrap {
  position: relative;
}

.form-select {
  width: 100%;
  padding: 9px 36px 9px 12px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  font-size: 0.875rem;
  color: #1a202c;
  background: #fff;
  appearance: none;
  cursor: pointer;
  transition: border-color 0.15s;
}

.form-select:focus {
  outline: none;
  border-color: #06195e;
}

.select-arrow {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  pointer-events: none;
  color: #718096;
  font-size: 0.75rem;
}

.site-info-card {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 18px 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  align-self: flex-start;
}

.site-info-row {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.site-info-label {
  font-size: 0.75rem;
  font-weight: 700;
  color: #718096;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.site-info-value {
  font-size: 0.875rem;
  color: #1a202c;
  line-height: 1.5;
}

.schedule-notice {
  background: #fffbeb;
  border: 1px solid #f6e05e;
  border-radius: 6px;
  padding: 10px 16px;
  font-size: 0.82rem;
  color: #744210;
  margin-bottom: 16px;
  text-align: center;
}

.selected-schedule-row {
  display: flex;
  gap: 10px;
  align-items: baseline;
  margin-bottom: 14px;
}

.selected-schedule-label {
  font-size: 0.82rem;
  font-weight: 600;
  color: #4a5568;
}

.selected-schedule-value {
  font-size: 0.9rem;
  font-weight: 700;
  color: #2b4b9e;
}

.date-time-label {
  font-size: 0.82rem;
  font-weight: 600;
  color: #4a5568;
  margin-bottom: 8px;
}

.earliest-badge {
  display: inline-block;
  background: #e6ffed;
  border: 1px solid #9ae6b4;
  color: #276749;
  font-size: 0.78rem;
  font-weight: 600;
  border-radius: 6px;
  padding: 4px 10px;
  margin-bottom: 16px;
}

.calendar-time-grid {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 24px;
  align-items: flex-start;
}

.calendar-wrapper {
  background: #fff;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  padding: 16px 18px;
  min-width: 280px;
  max-width: 320px;
}

.calendar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
}

.cal-nav {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.1rem;
  color: #718096;
  padding: 4px 8px;
  border-radius: 6px;
  transition: background 0.15s;
}

.cal-nav:hover {
  background: #f0f4f9;
  color: #06195e;
}

.cal-month-label {
  font-size: 0.9rem;
  font-weight: 700;
  color: #1a202c;
  letter-spacing: 0.02em;
}

.cal-weekdays {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 2px;
  margin-bottom: 6px;
}

.cal-weekdays span {
  text-align: center;
  font-size: 0.72rem;
  font-weight: 600;
  color: #a0aec0;
  padding: 4px 0;
}

.cal-days {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 3px;
}

.cal-day {
  aspect-ratio: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.82rem;
  border-radius: 50%;
  border: none;
  cursor: pointer;
  transition: all 0.15s;
  font-weight: 500;
}

.cal-day.blank {
  background: none;
  cursor: default;
}

.cal-day.available {
  background: #e6f4ea;
  color: #276749;
  font-weight: 600;
}

.cal-day.available:hover {
  background: #c6e6cc;
}

.cal-day.unavailable {
  background: #f1f5f9;
  color: #c0c8d8;
  cursor: not-allowed;
}

.cal-day.selected {
  background: #06195e !important;
  color: #fff !important;
  box-shadow: 0 2px 8px rgba(6, 25, 94, 0.3);
}

.cal-day.today {
  outline: 2px solid #e53e3e;
  outline-offset: -2px;
}

.time-slots-wrapper {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.time-slot-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  border: 1.5px solid #e2e8f0;
  border-radius: 8px;
  cursor: pointer;
  transition:
    border-color 0.15s,
    background 0.15s;
}

.time-slot-row:hover {
  border-color: #a3b4e8;
  background: #f8faff;
}

.time-slot-row.selected {
  border-color: #06195e;
  background: #eef2fb;
}

.time-slot-label {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.time-radio-input {
  display: none;
}

.time-radio-circle {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: 2px solid #06195e;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.time-radio-dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: #06195e;
}

.time-slot-text {
  font-size: 0.875rem;
  font-weight: 500;
  color: #1a202c;
}

.slots-badge {
  font-size: 0.78rem;
  font-weight: 600;
}

.slots-available {
  color: #276749;
}

.slots-full {
  color: #c53030;
}

/* ── Tab 1: Application Type ─────────────────────────────────────── */
.apptype-schedule-bar {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 12px;
}

.apptype-schedule-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.apptype-schedule-label {
  font-size: 0.75rem;
  color: #718096;
  font-weight: 600;
}

.apptype-schedule-value {
  font-size: 0.9rem;
  font-weight: 700;
  color: #2b4b9e;
}

.apptype-notice {
  background: #ebf4ff;
  border: 1px solid #bee3f8;
  border-radius: 6px;
  padding: 10px 16px;
  font-size: 0.82rem;
  color: #2c5282;
  margin-bottom: 16px;
  text-align: center;
}

.apptype-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.apptype-divider {
  border: none;
  border-top: 1.5px solid #f0f4f9;
  margin: 22px 0;
}

.form-input {
  width: 100%;
  padding: 9px 12px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  font-size: 0.875rem;
  color: #1a202c;
  background: #fff;
  box-sizing: border-box;
  transition: border-color 0.15s;
}

.form-input:focus {
  outline: none;
  border-color: #06195e;
}

.toggle-group {
  display: flex;
  gap: 0;
}

.toggle-btn {
  padding: 7px 18px;
  font-size: 0.82rem;
  font-weight: 600;
  border: 1.5px solid #d1d9e6;
  background: #f8fafc;
  color: #718096;
  cursor: pointer;
  transition: all 0.15s;
}

.toggle-btn:first-child {
  border-radius: 6px 0 0 6px;
  border-right: none;
}

.toggle-btn:last-child {
  border-radius: 0 6px 6px 0;
}

.toggle-btn.active {
  background: #06195e;
  color: #fff;
  border-color: #06195e;
}

.toggle-btn:not(.active):hover {
  background: #eef2fb;
  border-color: #a3b4e8;
}

/* ── Tab 2: Companions & Assistants ─────────────────────────────── */
.ca-section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #1a202c;
  margin: 0 0 12px;
}

.ca-notice {
  background: #ebf4ff;
  border: 1px solid #bee3f8;
  border-radius: 6px;
  padding: 10px 16px;
  font-size: 0.82rem;
  color: #2c5282;
  text-align: center;
  margin-bottom: 14px;
}

.ca-input-row {
  display: flex;
  gap: 10px;
  align-items: center;
}

.ca-input-row .form-input {
  flex: 1;
}

.btn-ca-add {
  background: #06195e;
  color: #fff;
  padding: 9px 20px;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  border: none;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s;
}

.btn-ca-add:hover {
  background: #04134a;
}

.ca-list {
  margin-top: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.ca-list-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 9px 14px;
  background: #f8fafc;
  border: 1.5px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.875rem;
  color: #1a202c;
}

.ca-remove {
  background: none;
  border: none;
  color: #a0aec0;
  cursor: pointer;
  font-size: 0.8rem;
  padding: 2px 6px;
  border-radius: 4px;
  transition:
    color 0.15s,
    background 0.15s;
}

.ca-remove:hover {
  color: #e53e3e;
  background: #fff5f5;
}

/* ── Tab 3: Application Form ─────────────────────────────────────── */
.af-grid-3 {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px 24px;
}

.af-span-2 {
  grid-column: span 2;
}

.af-field {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.af-label {
  font-size: 0.72rem;
  font-weight: 700;
  color: #718096;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.af-value {
  font-size: 0.875rem;
  color: #1a202c;
  line-height: 1.4;
}

.af-family-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0 24px;
}

.af-family-col {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.af-family-role {
  font-size: 0.75rem;
  font-weight: 800;
  color: #4a5568;
  letter-spacing: 0.06em;
  margin-bottom: 4px;
}

.af-declaration-box {
  background: #fff5f5;
  border: 1.5px solid #feb2b2;
  border-radius: 10px;
  padding: 18px 20px;
  margin-bottom: 14px;
}

.af-declaration-label {
  display: flex;
  gap: 12px;
  cursor: pointer;
  align-items: flex-start;
}

.af-declaration-text p {
  font-size: 0.8rem;
  color: #742a2a;
  margin: 0 0 6px;
  line-height: 1.55;
}

.af-declaration-text p:last-child {
  margin-bottom: 0;
}

.af-certify-box {
  background: #fffff0;
  border: 1.5px solid #f6e05e;
  border-radius: 10px;
  padding: 18px 20px;
  margin-bottom: 14px;
}

.af-certify-title {
  font-size: 0.85rem;
  font-weight: 700;
  color: #744210;
  margin-bottom: 12px;
}

.af-certify-label {
  display: flex;
  gap: 12px;
  cursor: pointer;
  align-items: flex-start;
}

.af-certify-label .af-declaration-text p {
  color: #744210;
}

.af-note {
  font-size: 0.78rem;
  color: #744210;
  margin: 12px 0 0;
}

/* ── Tab 4: Documentary Requirements ────────────────────────────── */
.file-notice {
  background: #fdf8ec;
  border: 1px solid #f0d99a;
  color: #7a5c00;
  font-size: 11.5px;
  text-align: center;
  padding: 7px 12px;
  border-radius: 3px;
  margin-bottom: 18px;
}

.section-label {
  font-size: 12.5px;
  font-weight: 600;
  color: #333;
  margin-bottom: 14px;
}

.req-item {
  margin-bottom: 4px;
}

.req-item__label {
  display: block;
  font-size: 12px;
  color: #555;
  margin-bottom: 8px;
}

.upload-zone {
  margin-bottom: 2px;
}

.browse-btn {
  background: #1a6fc4;
  color: #fff;
  border: none;
  border-radius: 3px;
  font-size: 12px;
  padding: 6px 16px;
  cursor: pointer;
  font-weight: 500;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  transition: background-color 0.2s ease;
}

.browse-btn:hover {
  background: #155fa0;
}

.file-display {
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid #dce3ec;
  border-radius: 3px;
  padding: 5px 10px;
  margin-top: 7px;
  background: #f8fafc;
  max-width: 420px;
}

.file-name {
  flex: 1;
  font-size: 12px;
  color: #444;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.remove-btn {
  background: none;
  border: none;
  color: #d32f2f;
  cursor: pointer;
  padding: 0 2px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  transition: color 0.2s ease;
}

.remove-btn:hover {
  color: #b71c1c;
}

.hidden-input {
  display: none;
}

.divider {
  border: none;
  border-top: 1px solid #e8edf3;
  margin: 14px 0 10px;
}

/* Conflict Modal */
.modal-box {
  background: #fff;
  border-radius: 6px;
  width: 580px;
  max-width: 96vw;
  max-height: 88vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.18);
  overflow-y: auto;
}

.modal-small {
  width: 360px;
}

.modal-banner {
  background: #fdf8ec;
  border-bottom: 1px solid #f0d99a;
  color: #7a5c00;
  font-size: 12.5px;
  text-align: center;
  padding: 11px 18px;
  border-radius: 6px 6px 0 0;
}

.modal-body {
  padding: 16px 20px 8px;
}

.conflict-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 12.5px;
  margin-bottom: 6px;
}

.conflict-table thead th {
  text-align: left;
  font-weight: 700;
  color: #333;
  padding: 5px 12px 9px;
  border-bottom: 1px solid #e0e0e0;
}

.conflict-table thead th:first-child {
  width: 24%;
}

.conflict-table tbody td {
  padding: 7px 12px;
  vertical-align: top;
  border-bottom: 0.5px solid #f0f0f0;
  color: #555;
  font-size: 12px;
  line-height: 1.45;
}

.conflict-table tbody td.field-name {
  font-weight: 600;
  color: #333;
  white-space: nowrap;
}

.id-preview-wrap {
  width: 80%;
  margin: 14px auto 16px;
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid #ccc;
  background: #f0f0f0;
  min-height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.id-preview-img {
  width: 100%;
  display: block;
  border-radius: 5px;
}

.id-preview-placeholder {
  font-size: 12px;
  color: #999;
  padding: 20px;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: center;
  padding: 6px 20px 20px;
}

.btn-confirm {
  background: #388e3c;
  color: #fff;
  border: none;
  border-radius: 4px;
  font-size: 13px;
  font-weight: 600;
  padding: 9px 24px;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.btn-confirm:hover {
  background: #2e7d32;
}

.btn-reupload {
  background: #d32f2f;
  color: #fff;
  border: none;
  border-radius: 3px;
  font-size: 13px;
  font-weight: 600;
  padding: 9px 24px;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.btn-reupload:hover {
  background: #b71c1c;
}

/* ── Tab 5: Payment ──────────────────────────────────────────────── */
.appt-title {
  font-size: 14px;
  font-weight: 700;
  color: #222;
  margin-bottom: 20px;
}

.hname {
  color: #1a6fc4;
  font-weight: 700;
}

.group-label {
  font-size: 12.5px;
  font-weight: 600;
  color: #333;
  margin-bottom: 8px;
}

.payment-group {
  margin-bottom: 22px;
}

.radio-card {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  border: 1px solid #dde3ec;
  border-radius: 3px;
  padding: 10px 14px;
  margin-bottom: 6px;
  cursor: pointer;
  background: #fff;
  transition: background 0.15s;
}

.radio-card:hover {
  background: #f5f8fd;
}

.radio-card.selected {
  background: #eef4fd;
  border-color: #aac8f0;
}

.radio-card input[type="radio"] {
  margin-top: 3px;
  accent-color: #1a6fc4;
  width: fit-content;
}

.radio-card__content {
  width: 100%;
}

.radio-card__title {
  font-size: 12.5px;
  font-weight: 600;
  color: #222;
  margin-bottom: 2px;
}

.radio-card__sub {
  font-size: 11.5px;
  color: #666;
  margin: 0;
  line-height: 1.5;
}

.summary-section {
  margin-top: 6px;
}

.summary-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 12.5px;
  margin-bottom: 16px;
}

.summary-table thead th {
  text-align: left;
  font-weight: 600;
  color: #555;
  font-size: 12px;
  padding: 8px 12px;
  border: 1px solid #dde3ec;
  background: #f7f9fc;
}

.summary-table tbody td {
  padding: 8px 12px;
  border: 1px solid #dde3ec;
  color: #333;
  font-size: 12.5px;
}

.summary-table .text-right {
  text-align: right;
}

.summary-table .total-row td {
  background: #f7f9fc;
  font-size: 12.5px;
}

.pay-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 8px;
}

.btn-proceed-pay {
  background: #1a6fc4;
  color: #fff;
  border: none;
  border-radius: 3px;
  font-size: 12.5px;
  font-weight: 600;
  padding: 8px 22px;
  cursor: pointer;
}

.btn-proceed-pay:hover {
  background: #155fa0;
}

/* Status view */
.status-intro {
  font-size: 12.5px;
  color: #555;
  margin-bottom: 14px;
}

.status-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 20px;
}

.status-table td {
  padding: 10px 0;
  border-bottom: 1px solid #eee;
  font-size: 13px;
}

.st-label {
  color: #555;
  font-weight: 500;
  width: 200px;
}

.st-value {
  color: #222;
  text-align: right;
}

.st-initiated {
  color: #1a6fc4;
  font-weight: 600;
}

.st-green {
  color: #2e7d32;
  font-weight: 600;
}

.guidelines-title {
  font-size: 12.5px;
  font-weight: 600;
  color: #333;
  margin-bottom: 8px;
}

.guidelines-list {
  font-size: 12px;
  color: #444;
  padding-left: 20px;
  line-height: 1.7;
  margin-bottom: 14px;
}

.guidelines-list li {
  margin-bottom: 4px;
}

.guidelines-link {
  font-size: 12px;
  color: #555;
  margin-bottom: 16px;
}

.link-blue {
  color: #1a6fc4;
  text-decoration: underline;
  cursor: pointer;
}

.status-nav {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 16px;
}

.btn-back-pay {
  background: #fff;
  color: #555;
  border: 1px solid #bbb;
  border-radius: 3px;
  font-size: 12px;
  padding: 6px 20px;
  cursor: pointer;
}

.btn-back-pay:hover {
  background: #f0f0f0;
}

.btn-pay-now {
  background: #1a6fc4;
  color: #fff;
  border: none;
  border-radius: 3px;
  font-size: 12px;
  font-weight: 600;
  padding: 6px 20px;
  cursor: pointer;
}

.btn-pay-now:hover {
  background: #155fa0;
}

/* ── Payment Modals ──────────────────────────────────────────────── */
.payment-modal {
  width: 580px;
}

.modal-title {
  font-size: 15px;
  font-weight: 700;
  color: #222;
  text-align: center;
  padding: 18px 22px 0;
  margin: 0;
}

.modal-scroll-body {
  overflow-y: auto;
  padding: 14px 24px 10px;
  flex: 1;
}

.modal-remind {
  font-size: 12.5px;
  color: #333;
  margin-bottom: 8px;
  margin-top: 12px;
}

.modal-list {
  font-size: 12px;
  color: #444;
  padding-left: 20px;
  line-height: 1.7;
  margin-bottom: 6px;
}

.modal-list li {
  margin-bottom: 4px;
}

.modal-foot {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 12px 22px 18px;
  border-top: 1px solid #eee;
}

.modal-foot--center {
  justify-content: center;
  border-top: none;
  padding-top: 6px;
}

.btn-modal-cancel {
  background: #fff;
  color: #555;
  border: 1px solid #bbb;
  border-radius: 3px;
  font-size: 12.5px;
  padding: 7px 22px;
  cursor: pointer;
}

.btn-modal-cancel:hover {
  background: #f0f0f0;
}

.btn-modal-agree {
  background: #1a6fc4;
  color: #fff;
  border: none;
  border-radius: 3px;
  font-size: 12.5px;
  font-weight: 600;
  padding: 7px 22px;
  cursor: pointer;
}

.btn-modal-agree:hover {
  background: #155fa0;
}

/* Success modal */
.success-icon-wrap {
  display: flex;
  justify-content: center;
  padding: 22px 0 8px;
}

.success-sub {
  font-size: 13px;
  color: #555;
  text-align: center;
  margin: 0 0 14px;
}

/* DFA ePayment Receipt */
.dfa-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 18px 22px 12px;
  border-bottom: 1px solid #eee;
}

.dfa-dept {
  font-size: 12px;
  font-weight: 700;
  color: #222;
  margin: 0;
}

.dfa-epay {
  font-size: 12px;
  color: #1a6fc4;
  font-weight: 600;
  margin: 0;
}

.epay-intro {
  font-size: 12px;
  color: #666;
  padding: 10px 22px 4px;
  margin: 0;
}

.epay-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.epay-table td {
  padding: 10px 22px;
  border-bottom: 1px solid #f0f0f0;
}

.ep-label {
  color: #555;
  font-size: 12.5px;
}

.ep-value {
  color: #222;
  text-align: right;
  font-size: 13px;
}

.ep-bold {
  font-weight: 700;
}

.ep-green {
  color: #2e7d32;
  font-weight: 700;
}

.not-receipt {
  font-size: 12px;
  color: #e0900a;
  text-align: center;
  padding: 12px 22px 4px;
  margin: 0;
}

/* ── Responsive ──────────────────────────────────────────────────── */
@media (max-width: 992px) {
  .app-layout {
    grid-template-columns: 80px 1fr;
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
  }

  .step-label {
    display: none;
  }

  .nav-buttons {
    flex-wrap: wrap;
    gap: 12px;
  }

  .terms-card {
    padding: 24px 20px;
  }

  .modal-card {
    padding: 24px 20px;
  }

  .site-location-grid {
    grid-template-columns: 1fr;
  }

  .calendar-time-grid {
    grid-template-columns: 1fr;
  }

  .apptype-row {
    grid-template-columns: 1fr;
  }

  .af-grid-3 {
    grid-template-columns: repeat(2, 1fr);
  }

  .af-family-grid {
    grid-template-columns: 1fr;
  }
}
</style>
