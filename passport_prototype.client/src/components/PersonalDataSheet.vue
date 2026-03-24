<template>
  <!-- Hidden Input for File Selection (place this anywhere in your template) -->
  <input type="file"
         ref="requiredDocFile"
         @change="handleGenericFileSelection"
         style="display: none;"
         accept=".pdf, .jpg, .jpeg, .png" />

  <div class="app-layout">
    <LeftMenu class="leftMenu" />

    <Header title="Personal Data Sheet" class="header" />

    <div class="dashboard-content">

      <h2 class="page-title">Personal Data Sheet Management</h2>

      <div class="pds-top-bar">
        <div class="tab-container">
          <button v-for="tab in ['Personal', 'Family', 'Contact', 'Work']"
                  :key="tab"
                  :class="['tab-btn', { active: activeTab === tab }]"
                  @click="activeTab = tab">
            {{ tab }}
          </button>
        </div>
      </div>

      <div class="tab-wrapper">
        <div class="tab-content">
          <transition name="fade-slide" mode="out-in">
            <div :key="activeTab" class="form-wrapper">

              <!-- Personal -->
              <div v-if="activeTab === 'Personal'">
                <!-- fields -->
              </div>


              <div v-else-if="activeTab === 'Family'">
       
          
              </div>

      <!--asdd -->
      <div v-else-if="activeTab === 'Contact'">
        <!-- Personal Phone Numbers Header -->
        <div style="margin-bottom: 30px;">
          <div style="display: flex; align-items: center; gap: 15px; margin-bottom: 20px;">
            <p style="margin: 0; font-weight: bold; white-space: nowrap;">Personal Phone Numbers</p>
          </div>

          <!-- YOUR EXACT ADDRESS FIELDS FIRST -->
          <div style="display: flex; flex-direction: column; gap: 12px;">
            <label class="form-row">
              <span>Current Address</span>
              <input class="auth-input" v-model="address.address" />
            </label>
            <label class="form-row">
              <span>Address Abroad</span>
              <input class="auth-input" v-model="address.abroad" />
            </label>
            <label class="form-row">
              <span>Current Address</span>
              <input class="auth-input" v-model="address.address" />
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

          <!-- Mobile (SA IBABA ng Address) -->
          <label class="form-row" style="margin-top: 24px;">
            <span>Personal Mobile</span>
            <div class="phone-input-group">
              <select class="phone-flag" v-model="contact.mobileCountry">
                <option value="+63">🇵🇭 +63</option>
                <option value="+1">🇺🇸 +1</option>
                <option value="+44">🇬🇧 +44</option>
              </select>
              <input class="auth-input phone-prefix" placeholder="906" v-model="contact.mobilePrefix" maxlength="4" />
              <input class="auth-input phone-number" placeholder="1234567" v-model="contact.mobileNumber" />
            </div>
          </label>

          <!-- Landline (SA IBABA ng Mobile) -->
          <label class="form-row" style="margin-top: 16px;">
            <span>Personal Landline</span>
            <div class="phone-input-group">
              <select class="phone-flag" v-model="contact.landlineCountry">
                <option value="+63">🇵🇭 +63</option>
                <option value="+1">🇺🇸 +1</option>
                <option value="+44">🇬🇧 +44</option>
              </select>
              <input class="auth-input phone-number" placeholder="02-123-4567" v-model="contact.landlineNumber" />
            </div>
          </label>
          <div class="button-group-row">
            <button @click="save" class="btn" style="margin-left: auto; width: 200px;">
              Save Progress
            </button>
          </div>
        </div>
      </div>


        <div v-else-if="activeTab === 'Work'">
          <div class="form-wrapper">
            <!-- Personal Phone Numbers Header -->
            <div style="margin-bottom: 30px;">
              <div style="display: flex; align-items: center; gap: 15px; margin-bottom: 20px;">
                <p style="margin: 0; font-weight: bold; white-space: nowrap;">Work Information</p>
              </div>

              <!-- YOUR EXACT ADDRESS FIELDS FIRST -->
              <div style="display: flex; flex-direction: column; gap: 12px;">
                <label class="form-row">
                  <span>Occuption</span>
                  <input class="auth-input" v-model="address.address" />
                </label>
                <label class="form-row">
                  <span>Office Address</span>
                  <input class="auth-input" v-model="address.abroad" />
                </label>
                <label class="form-row">
                  <span>Office Country</span>
                  <input class="auth-input" v-model="address.address" />
                </label>
                <label class="form-row">
                  <span>Office Region</span>
                  <select class="auth-select" v-model="address.country">
                    <option value="">Select Country</option>
                  </select>
                </label>
                <label class="form-row">
                  <span>Office Province</span>
                  <select class="auth-select" v-model="address.region">
                    <option value="">Select Region</option>
                  </select>
                </label>
                <label class="form-row">
                  <span>Office Municipality</span>
                  <select class="auth-select" v-model="address.province">
                    <option value="">Select Province</option>
                  </select>
                </label>
                <label class="form-row">
                  <span>Office Barangay</span>
                  <select class="auth-select" v-model="address.municipality">
                    <option value="">Select Municipality</option>
                  </select>
                </label>
                <label class="form-row">
                  <span>Office postal code</span>
                  <select class="auth-select" v-model="address.barangay">
                    <option value="">Select Barangay</option>
                  </select>
                </label>
              </div>

              <!-- Mobile (SA IBABA ng Address) -->
              <label class="form-row" style="margin-top: 24px;">
                <span>Personal Mobile</span>
                <div class="phone-input-group">
                  <select class="phone-flag" v-model="contact.mobileCountry">
                    <option value="+63">🇵🇭 +63</option>
                    <option value="+1">🇺🇸 +1</option>
                    <option value="+44">🇬🇧 +44</option>
                  </select>
                  <input class="auth-input phone-prefix" placeholder="906" v-model="contact.mobilePrefix" maxlength="4" />
                  <input class="auth-input phone-number" placeholder="1234567" v-model="contact.mobileNumber" />
                </div>
              </label>

              <!-- Landline (SA IBABA ng Mobile) -->
              <label class="form-row" style="margin-top: 16px;">
                <span>Personal Landline</span>
                <div class="phone-input-group">
                  <select class="phone-flag" v-model="contact.landlineCountry">
                    <option value="+63">🇵🇭 +63</option>
                    <option value="+1">🇺🇸 +1</option>
                    <option value="+44">🇬🇧 +44</option>
                  </select>
                  <input class="auth-input phone-number" placeholder="02-123-4567" v-model="contact.landlineNumber" />
                </div>
              </label>
              <div class="button-group-row">
                <button @click="save" class="btn" style="margin-left: auto; width: 200px;">
                  Save Progress
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
          </transition>
        </div>
      </div>
    </div>
  </div>



  <!-- Reusable dialog -->
  <DialogBox :show="showDialog"
             :title="dialogTitle"
             :message="dialogMessage"
             @close="handleCloseDialog" />

  <!-- Loading Dialog (always on top of everything) -->
  <LoadingDialog :visible="isLoading" />
</template>

<script setup>import LeftMenu from '@/components/LeftMenu.vue'
  //import Header from './Header.vue'
  import api from "@/api";
  import { ref, onMounted, computed, watch } from "vue";
  import { useAuthStore } from "@/stores/auth";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "./LoadingDialog.vue";

  const apiBaseUrl = api.defaults.baseURL;

  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const isLoading = ref(false);

  const tabs = ["Personal Data Sheet 1", "Personal Data Sheet 2", "Personal Data Sheet 3", "Personal Data Sheet 4"];
  const activeTab = ref("Personal Data Sheet 1");

  const auth = useAuthStore();
  const personId = computed(() => auth.userId); // logged in user’s ID
  const personEmail = computed(() => auth.email);
  const birthdate = computed(() => auth.birthdate);
  const employeeID = computed(() => auth.employeeID);
  const firstName = computed(() => auth.firstName);
  const lastName = computed(() => auth.lastName);

const contact = ref({
  mobile: '',
  landline: '',
  email: ''
})

const address = ref({
  street: '',
  city: '',
  province: '',
  region: '',
  postalCode: '',
  country: ''
})

  // ------------------ State ------------------
  const user = ref({
    personID: personId.value,
    csidNo: "",
    surname: lastName.value,
    firstName: firstName.value,
    middleName: "",
    nameExtension: "",
    dateOfBirth: birthdate.value,
    placeOfBirth: "",
    bloodType: "",
    nationality: "",
    civilStatusID: null,

    departmentID: null,
    designation: "",

    sexID: null,
    heightCM: 0,
    weightKG: 0,
    gsisID: "",
    pagibigID: "",
    philhealthID: "",
    sssNo: "",
    tin: "",
    agencyEmployeeNo: employeeID.value,
    residentialAddress: "",
    residentialZip: "",
    permanentAddress: "",
    permanentZip: "",
    telephoneNo: "",
    mobileNo: "",
    email: personEmail.value,

    birthCertificateFileKey: "",
    marriageCertificateFileKey: "",

    // NEW FIELDS TO STORE THE ORIGINAL FILENAME
    birthCertificateOriginalName: "", // <-- NEW
    marriageCertificateOriginalName: "", // <-- NEW

    // 🔹 Spouse
    spouseSurname: "",
    spouseFirstName: "",
    spouseMiddleName: "",
    spouseNameExtension: "",
    spouseOccupation: "",
    spouseEmployer: "",
    spouseBusinessAddress: "",
    spouseTelephone: "",

    // 🔹 Father
    fatherSurname: "",
    fatherFirstName: "",
    fatherMiddleName: "",
    fatherNameExtension: "",

    // 🔹 Mother
    motherSurname: "",
    motherFirstName: "",
    motherMiddleName: "",
    motherNameExtension: "",

    // 🔹 PDS 4: Other Information & References (Based on standard PDS)
    specialSkills: [], // Collection
    nonAcademicDistinctions: [], // Collection
    membershipInAssociation: [], // Collection
    references: [], // Collection

    // 🔹 PDS 4: Specific Fields (e.g., questions in Box 34-39 in old PDS)
    // Assuming these are checkboxes/booleans/strings for the required questions
    q35bDateFiled: null,
    q35bStatus: null,
    convictedFinalJudgment: null,
    convictedFinalDetails: null,
    hasPendingCase: null,
    pendingCaseDetails: null,
    isDualCitizen: null,
    dualCitizenMode: null,
    q39Country: null,

    areThirdDegree: false,
    areFourthDegree: false,
    guiltyAdministrativeOffense: false,
    criminallyCharged: false,
    convictedOfCrime: false,
    separatedFromService: false,
    candidateInElection: false,
    resignedFromGovt: false,
    immigrantOrResident: false,
    indigenousGroup: false,
    personWithDisability: false,
    soloParent: false,

    // Details for the above questions (Text boxes that appear if "YES")
    thirdFourthDegreeDetails: "",
    adminOffenseDetails: "",
    criminalChargeDetails: "",
    convictedCrimeDetails: "",
    separatedServiceDetails: "",
    candidateElectionDetails: "",
    resignedGovtDetails: "",
    immigrantResidentDetails: "",
    indigenousGroupDetails: "",
    pwdIDNo: "",
    soloParentIDNo: "",

    // Nested object for API mapping (will be populated in save() function)
    pdsSectionIV: null,


    // 🔹 PDS 5: DECLARATION FIELDS (Q. 42)
    declarationID: 0, // NEW: Maps to DeclarationID
    govIdType: "", // Maps to GovernmentIssuedID
    govIdNo: "", // Maps to LicensePassportIDNo
    idIssueDate: null, // Maps to DateOfIssuance
    idIssuePlace: "", // NEW: Maps to PlaceOfIssuance
    dateAccomplished: null, // Maps to DateAccomplished

    // Client-side fields that map to PDS IV/other models
    signature_text: "",
    subscribedDate: null,
    notary_name: "",

    // File Keys
    passportPhotoUrl: "", // Maps to PhotoFileKey
    passportPhotoOriginalName: "",
    signatureFileKey: "", // NEW: Maps to SignatureFileKey
    rightThumbmarkFileKey: "", // NEW: Maps to RightThumbmarkFileKey
    dateSchedule: null, // NEW: Maps to DateSchedule

    // 🔹 Collections (used for API communication)
    children: [],
    voluntaryWorks: [],
    trainings: [],
    skillsHobbies: [],
    distinctions: [],
    memberships: [],
    civilServiceEligibilities: [],
    workExperiences: [],

    // We still need the 'references' collection
    references: [],
  });

  const genders = ref([]);
  const civilStatuses = ref([]);

  const departments = ref([]);

  const c2 = ref([]);
  const c3 = ref([]);
  const c4 = ref({ other: [], references: [] });

  const showValidationErrors = ref(false);

  // Add this function to your <script setup>
  const addEducationRow = () => {
    user.value.educationRecords.push({
      educationID: 0,
      educationLevelID: null, // Use this name consistently
      schoolName: "",
      degree: "",
      attendanceFrom: null,
      attendanceTo: null,
      highestLevelUnits: "", // Updated to match your mapping
      yearGraduated: "",
      scholarshipAcademicHonors: "", // Updated to match your mapping
      orderIndex: user.value.educationRecords.length // Fixed 'this' error
    });
  };

  const validatePDS = () => {
    // --- PDS SHEET 1: BASIC EMPLOYMENT INFORMATION ---
    // Added validation for Department and Designation
    if (!user.value.departmentID) {
      return {
        isValid: false,
        message: "Personal Information: Please select a Department in Personal Data Sheet 1."
      };
    }

    if (!user.value.designation || !user.value.designation.trim()) {
      return {
        isValid: false,
        message: "Personal Information: Please enter your Designation (Position Title) in Personal Data Sheet 1."
      };
    }

    // --- PDS SHEET 1: EDUCATIONAL BACKGROUND VALIDATION ---
    if (user.value.educationRecords && user.value.educationRecords.length > 0) {
      for (const [i, edu] of user.value.educationRecords.entries()) {
        if (
          !edu.educationLevelID ||
          !edu.schoolName ||
          !edu.degree ||
          !edu.attendanceFrom ||
          !edu.attendanceTo
        ) {
          return {
            isValid: false,
            message: `Educational Background Row ${i + 1}: Please fill all fields (Level, School, Degree, and Years) in Personal Data Sheet 1.`
          };
        }
      }
    }

    // --- PDS SHEET 2 VALIDATION ---
    for (const [i, e] of user.value.civilServiceEligibilities.entries()) {
      if (!e.careerService || !e.rating || !e.dateOfExamination || !e.placeOfExamination || !e.licenseNumber || !e.licenseValidity || !e.dateReleased) {
        return { isValid: false, message: `Eligibility Row ${i + 1}: Please fill all fields in Personal Data Sheet 2.` };
      }
    }

    for (const [i, w] of user.value.workExperiences.entries()) {
      if (!w.dateFrom || !w.dateTo || !w.positionTitle || !w.departmentAgencyCompany || !w.monthlySalary || !w.salaryGradeStep || !w.statusOfAppointment) {
        return { isValid: false, message: `Work Experience Row ${i + 1}: Please fill all fields in Personal Data Sheet 2.` };
      }
    }

    // --- PDS SHEET 3 VALIDATION ---
    for (const [i, v] of user.value.voluntaryWorks.entries()) {
      if (!v.organization || !v.dateFrom || !v.dateTo || !v.numberOfHours || !v.position) {
        return { isValid: false, message: `Voluntary Work Row ${i + 1}: Please fill all fields in Personal Data Sheet 3.` };
      }
    }

    for (const [i, t] of user.value.trainings.entries()) {
      if (!t.title || !t.dateFrom || !t.dateTo || !t.numberOfHours || !t.typeOfLD || !t.conductedBy) {
        return { isValid: false, message: `Training Row ${i + 1}: Please fill all fields in Personal Data Sheet 3.` };
      }
    }

    for (const [i, s] of user.value.skillsHobbies.entries()) {
      if (!s.skillOrHobby) return { isValid: false, message: `Skills/Hobbies Row ${i + 1} is empty in Personal Data Sheet 3.` };
    }
    for (const [i, d] of user.value.distinctions.entries()) {
      if (!d.distinction) return { isValid: false, message: `Distinctions Row ${i + 1} is empty in Personal Data Sheet 3.` };
    }
    for (const [i, m] of user.value.memberships.entries()) {
      if (!m.organizationName) return { isValid: false, message: `Membership Row ${i + 1} is empty in Personal Data Sheet 3.` };
    }

    // --- PDS SHEET 4: REFERENCES VALIDATION ---
    //if (!user.value.references || user.value.references.length < 3) {
    //  return {
    //    isValid: false,
    //    message: "References: Please provide at least three (3) references in Personal Data Sheet 4."
    //  };
    //}

    for (const [i, ref] of user.value.references.entries()) {
      if (!ref.name || !ref.address || !ref.telephoneNo) {
        return {
          isValid: false,
          message: `Reference Row ${i + 1}: Please fill in the Name, Address, and Contact Number in Personal Data Sheet 4.`
        };
      }
    }

    return { isValid: true, message: "" };
  };

  // ------------------ Mapping Functions ------------------

  // Example data loading function structure
  const loadPdsData = async (personId) => {
    // ... (API fetch call here)
    const apiResponse = await response.json();

    // ✅ CONSOLE LOG: Logging BEFORE mapping fields back to the template
    console.log('--- INBOUND EducationRecords (From API) ---');
    console.log(apiResponse.educationRecords); // Adjust property name if needed
  };

  const educationLevels = ref([]);
  // Add these to your reactive variables
  const showAppointmentModal = ref(false);
  const appointmentDate = ref("");
  const appointmentTime = ref(""); // Optional: if you want specific slots
  // Add this helper function to your script setup
  const formatDate = (dateString) => {
    if (!dateString) return "Not Scheduled";
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'long',
      day: 'numeric',
      year: 'numeric'
    });
  };

  const openAppointmentModal = () => {
    if (user.value?.appointmentDate) {
      appointmentDate.value = user.value.appointmentDate.split('T')[0];
    } else {
      appointmentDate.value = ""; // Reset for new entries
    }
    showAppointmentModal.value = true;
  };

  const handleScheduleAppointment = async () => {
    // 1. Basic check for empty value
    if (!appointmentDate.value) {
      dialogTitle.value = "Required Field";
      dialogMessage.value = "Please select a preferred date.";
      showDialog.value = true;
      return;
    }

    // 2. PASTE HERE: Weekend Validation
    const day = new Date(appointmentDate.value).getUTCDay();
    if (day === 0 || day === 6) {
      dialogTitle.value = "Invalid Date";
      dialogMessage.value = "Appointments are only available Monday to Friday. Please select a weekday.";
      showDialog.value = true;
      return;
    }

    // 3. Proceed to API call
    isLoading.value = true;
    try {
      const payload = {
        preferredDate: new Date(appointmentDate.value).toISOString()
      };

      await api.post('/employee/my-schedule/request', payload);

      if (user.value) {
        user.value.appointmentDate = appointmentDate.value;
        user.value.status = 7;
      }

      showAppointmentModal.value = false;
      dialogTitle.value = "Schedule Requested";
      dialogMessage.value = "Your request has been submitted. Please wait for approval.";
      showDialog.value = true;

    } catch (err) {
      dialogTitle.value = "Request Failed";
      dialogMessage.value = err.response?.data?.message || "Failed to submit request.";
      showDialog.value = true;
    } finally {
      isLoading.value = false;
    }
  };

  // Add this to your existing refs
  const appointmentInfo = ref({
    dateSchedule: null,
    status: null
  });

  // Function to fetch schedule from your new API
  const fetchAppointmentSchedule = async () => {
    try {
      const res = await api.get('/employee/my-schedule/date');
      if (res.data) {
        appointmentInfo.value = res.data;

        // Ensure user.value exists before assigning to prevent "cannot set property of null"
        if (user.value) {
          // Match the API property 'dateSchedule' to your template property 'appointmentDate'
          user.value.appointmentDate = res.data.dateSchedule;
          user.value.status = res.data.status;
        }
      }
    } catch (err) {
      console.error("No appointment found:", err);
    }
  };

  onMounted(async () => {
    isLoading.value = true; // Set global loading to true immediately

    try {
      // Run these in parallel so the screen only "unlocks" once everything is ready
      await Promise.all([
        loadLookups(),
        loadData(),
        fetchAppointmentSchedule()
      ]);
    } catch (err) {
      console.error("Initialization failed", err);
    } finally {
      // Only now will the UI potentially show validation errors
      isLoading.value = false;
    }
  });


  // ------------------ Load Data ------------------
  // Load dropdown options
  async function loadLookups() {
    try {
      isLoading.value = true;

      // Run all lookup requests
      const [g, cs, dpt, edu] = await Promise.all([
        api.get("/employee/gender"),
        api.get("/employee/civilstatus"),
        api.get("/employee/departments"),
        api.get("/employee/educationlevel") // Added this line
      ]);

      genders.value = g.data;
      civilStatuses.value = cs.data;
      departments.value = dpt.data;
      educationLevels.value = edu.data; // Store the new levels

    } catch (err) {
      console.error("AxiosError:", err);

      if (err.response) {
        console.error("🔴 Backend responded with error:", err.response.status);
      } else {
        console.error("🔴 Error:", err.message);
      }

      showDialog.value = true;
      dialogTitle.value = "Error";
      dialogMessage.value = "Failed to load form options. Please refresh or log in again.";

    } finally {
      isLoading.value = false;
    }
  }

  async function loadData() {
    try {
      isLoading.value = true;
      const res = await api.get(`employee/user/${personId.value}`);
      const data = res.data;

      console.log("Raw API Data Object:", data);

      // helper to normalize dates for <input type="date">
      const formatDate = (dateStr) => {
        if (!dateStr) return "";
        return dateStr.substring(0, 10); // "yyyy-MM-dd"
      };

      // Ensures data from the API (1, 0, '1', or '0') is a definitive JS boolean (true/false)
      const convertToBoolean = (val) => {
        // Check for ALL true values
        if (val === true || val === 'true' || val === 1 || val === '1') return true;
        // Default to false for null, undefined, 0, '0', or false
        return false;
      };

      // -----------------------------------------------------------
      // PDS SECTION IV Mapping & C4 Mapping
      // -----------------------------------------------------------
      const pdsIVData = data.pdsSectionIV || {};
      const declarationData = data.declaration || {};

      // 1. Get the ID, using the correct camelCase key
      let declarationIDForState = declarationData.declarationID ?? 0;

      // 2. Check if a record exists (i.e., not a new, empty form)
      const declarationExists = declarationData.GovernmentIssuedID || declarationData.DateAccomplished;

      // 🛑 CRITICAL CONSOLIDATED LOGIC:
      // If the ID is 0 BUT the record contains data (declarationExists is true),
      // it means the backend is sending a faulty 0. We MUST force it to the known personID.
      if (declarationIDForState === 0 && declarationExists) {
        // We use the ID retrieved from the top level of the API response (data.personID)
        // as the definitive key for this record.
        declarationIDForState = data.personID;
        console.log("Debug: Patched faulty declaration ID to:", declarationIDForState);
      }

      if (
        declarationData.photoFileKey &&
        !declarationData.photoOriginalName
      ) {
        // Use the local state's name (which was correct before save cleanup)
        // to temporarily patch the incoming data.
        declarationData.photoOriginalName = user.value.passportPhotoOriginalName;
        console.log("Debug: Patched Passport Photo Original Name from local state:", declarationData.photoOriginalName);
      }

      // Mapping for user.value (All PDS 1-4 Fields)
      Object.assign(user.value, {
        // 🔹 PDS 1/Personal Info Fields
        personID: data.personID,
        csidNo: data.csidNo ?? "",
        surname: data.surname ?? "",
        firstName: data.firstName ?? "",
        middleName: data.middleName ?? "",
        nameExtension: data.nameExtension ?? "",
        dateOfBirth: formatDate(data.dateOfBirth),
        placeOfBirth: data.placeOfBirth ?? "",
        bloodType: data.bloodType ?? "",
        citizenship: data.citizenship ?? "",
        civilStatusID: data.civilStatusID ?? null,
        sexID: data.sexID ?? null,

        // Birth Certificate
        birthCertificateFileKey: data.birthCertificateFileKey || data.BirthCertificateFileKey || "",
        birthCertificateOriginalName: data.birthCertificateOriginalName || data.BirthCertificateOriginalName || "",

        // Marriage Certificate
        marriageCertificateFileKey: data.marriageCertificateFileKey || data.MarriageCertificateFileKey || data.marriageDocumentFileKey || "",
        marriageCertificateOriginalName: data.marriageCertificateOriginalName || data.MarriageCertificateOriginalName || data.marriageDocumentOriginalName || "",

        departmentID: data.departmentID ?? null,
        designation: data.designation ?? null,

        heightCM: data.heightCM ?? null,
        weightKG: data.weightKG ?? null,
        gsisID: data.gsisID ?? "",
        pagibigID: data.pagibigID ?? "",
        philhealthID: data.philhealthID ?? "",
        sssNo: data.sssNo ?? "",
        tin: data.tin ?? "",
        agencyEmployeeNo: data.agencyEmployeeNo ?? "",
        residentialAddress: data.residentialAddress ?? "",
        residentialZip: data.residentialZip ?? "",
        permanentAddress: data.permanentAddress ?? "",
        permanentZip: data.permanentZip ?? "",
        isSameAddress: convertToBoolean(data.isSameAddress),
        telephoneNo: data.telephoneNo ?? "",
        mobileNo: data.mobileNo ?? "",
        email: data.email ?? "",

        // 🔹 PDS 1 - Family Background
        spouseSurname: data.spouseSurname ?? "",
        spouseFirstName: data.spouseFirstName ?? "",
        spouseMiddleName: data.spouseMiddleName ?? "",
        spouseNameExtension: data.spouseNameExtension ?? "",
        spouseOccupation: data.spouseOccupation ?? "",
        spouseEmployer: data.spouseEmployer ?? "",
        spouseBusinessAddress: data.spouseBusinessAddress ?? "",
        spouseTelephone: data.spouseTelephone ?? "",

        fatherSurname: data.fatherSurname ?? "",
        fatherFirstName: data.fatherFirstName ?? "",
        fatherMiddleName: data.fatherMiddleName ?? "",
        fatherNameExtension: data.fatherNameExtension ?? "",

        motherSurname: data.motherSurname ?? "",
        motherFirstName: data.motherFirstName ?? "",
        motherMiddleName: data.motherMiddleName ?? "",
        motherNameExtension: data.motherNameExtension ?? "",

        // Dynamic Education Records Mapping
        educationRecords: (data.educationRecords ?? []).map(e => {
          // 1. Match the string from backend (e.g., "College") to the ID in your lookup
          // We use .toUpperCase() to prevent "college" vs "College" mismatch issues
          const matchedLevel = educationLevels.value.find(l =>
            l.display_Name?.trim().toUpperCase() === e.level?.trim().toUpperCase()
          );

          return {
            educationID: e.educationID ?? 0,
            // 2. Set educationLevelID so the <select v-model> can find the match
            educationLevelID: matchedLevel ? matchedLevel.level_ID : null,
            schoolName: e.schoolName ?? "",
            degree: e.degree ?? "",
            attendanceFrom: e.attendanceFrom ?? "",
            attendanceTo: e.attendanceTo ?? "",
            yearGraduated: e.yearGraduated ?? "",
            highestLevelUnits: e.highestLevel ?? "",
            scholarshipAcademicHonors: e.honors ?? ""
          };
        }),

        // ---------------------------------------------------------------------
        // 🔹 PDS Section IV Fields (Questions 34-40)
        // Mapped using the DTO property names (Q34a_RelatedWithin3rd, Q35b_DateFiled, etc.)
        // ---------------------------------------------------------------------

        // Q. 35b details
        q35bDateFiled: formatDate(pdsIVData.q35b_DateFiled) ?? null,
        q35bStatus: pdsIVData.q35b_Status ?? "",

        // Q. 34: Relatives
        areThirdDegree: convertToBoolean(pdsIVData.q34a_RelatedWithin3rd),
        areFourthDegree: convertToBoolean(pdsIVData.q34b_RelatedWithin4th),
        thirdFourthDegreeDetails: pdsIVData.q34_Details ?? "",

        // Q. 35: Admin/Criminal Charges
        guiltyAdministrativeOffense: convertToBoolean(pdsIVData.q35a_AdminOffense),
        criminallyCharged: convertToBoolean(pdsIVData.q35b_CriminallyCharged),
        adminOffenseDetails: pdsIVData.q35a_AdminDetails ?? "",
        criminalChargeDetails: pdsIVData.q35b_CriminalDetails ?? "",

        hasPendingCase: convertToBoolean(pdsIVData.HasPendingCase),
        pendingCaseDetails: pdsIVData.PendingCaseDetails ?? "",

        // Q. 36: Convicted of Crime
        convictedOfCrime: convertToBoolean(pdsIVData.q36_Convicted),
        convictedCrimeDetails: pdsIVData.q36_Details ?? "",

        convictedFinalJudgment: convertToBoolean(pdsIVData.ConvictedFinalJudgment),
        convictedFinalDetails: pdsIVData.ConvictedFinalDetails ?? "",

        // Q. 37: Separation from Service
        separatedFromService: convertToBoolean(pdsIVData.q37_Separated),
        separatedServiceDetails: pdsIVData.q37_Details ?? "",

        // Note: DTO does not explicitly map `resignedFromGovt` but keeping it for completeness if needed
        resignedFromGovt: convertToBoolean(pdsIVData.q38b_ResignedForCampaign),
        resignedGovtDetails: pdsIVData.q38b_Details ?? "",

        // Q. 38: Election History
        candidateInElection: convertToBoolean(pdsIVData.q38a_Candidate),
        candidateElectionDetails: pdsIVData.q38a_Details ?? "",

        // Q. 39: Immigrant/Dual Citizenship
        immigrantOrResident: convertToBoolean(pdsIVData.q39_Country),
        immigrantResidentDetails: pdsIVData.q39_Details_Country ?? "",

        // Q. 40: Other Information
        indigenousGroup: convertToBoolean(pdsIVData.q40a_IndigenousGroup),
        indigenousGroupDetails: pdsIVData.q40a_Details ?? "",
        personWithDisability: convertToBoolean(pdsIVData.q40b_Disability),
        pwdIDNo: pdsIVData.q40b_Details_IDNo ?? "",
        soloParent: convertToBoolean(pdsIVData.q40c_SoloParent),
        soloParentIDNo: pdsIVData.q40c_Details_IDNo ?? "",

        // ---------------------------------------------------------------------
        // 🔹 PDS Q. 42: Declaration
        // Prioritizes data.Declaration (new structure) and falls back to pdsIVData (legacy)
        // ---------------------------------------------------------------------
        declarationID: declarationIDForState,
        govIdType: declarationData.governmentIssuedID ?? "",
        govIdNo: declarationData.licensePassportIDNo ?? "",
        idIssueDate: formatDate(declarationData.dateOfIssuance),
        idIssuePlace: declarationData.placeOfIssuance ?? "",
        dateAccomplished: formatDate(declarationData.dateAccomplished),

        // File Keys (Photo, Signature, Thumbmark)
        passportPhotoUrl: declarationData.photoFileKey ?? "",
        passportPhotoOriginalName: declarationData.photoOriginalName ?? "",
        signatureFileKey: declarationData.signatureFileKey ?? "",
        rightThumbmarkFileKey: declarationData.rightThumbmarkFileKey ?? "",
        dateSchedule: formatDate(declarationData.dateSchedule),

        // Legacy PDS IV fields for Notarization
        signature_text: pdsIVData.Signature_Text ?? "", // Use pdsIVData for these fields
        subscribedDate: pdsIVData.SubscribedDate ?? null,
        notary_name: pdsIVData.NotaryName ?? "",
        // ---------------------------------------------------------------------

        // Collections: Map/Format collections (PDS 2/3)
        children: (data.children ?? []).map(c => ({
          ...c,
          dateOfBirth: formatDate(c.dateOfBirth),
        })),
        civilServiceEligibilities: (data.civilServiceEligibilities ?? []).map(c => ({
          ...c,
          dateOfExamination: formatDate(c.dateOfExamination),
          dateReleased: formatDate(c.dateReleased),
          licenseValidity: formatDate(c.licenseValidity),
        })),
        workExperiences: (data.workExperiences ?? []).map(w => ({
          ...w,
          dateFrom: formatDate(w.dateFrom),
          dateTo: formatDate(w.dateTo),
          salary: w.salary ?? null,
          isPresent: w.isPresent ?? false
        })),
        voluntaryWorks: (data.voluntaryWorks ?? []).map(v => ({
          ...v,
          dateFrom: formatDate(v.dateFrom),
          dateTo: formatDate(v.dateTo),
          numberOfHours: v.numberOfHours ?? null,
        })),
        trainings: (data.trainings ?? []).map(t => ({
          ...t,
          dateFrom: formatDate(t.dateFrom),
          dateTo: formatDate(t.dateTo),
          numberOfHours: t.numberOfHours ?? null,
        })),
        skillsHobbies: (data.skillsHobbies ?? []).map(s => ({
          ...s,
          skillOrHobby: s.skillOrHobby ?? ""
        })),
        distinctions: (data.distinctions ?? []).map(d => ({
          ...d,
          distinction: d.distinction ?? ""
        })),
        memberships: (data.memberships ?? []).map(m => ({
          ...m,
          organizationName: m.organizationName ?? ""
        })),
        references: (data.references ?? []).map(r => ({
          ...r,
          name: r.name ?? "",
          address: r.officeOrResidentialAddress ?? "",
          telephoneNo: r.contactNoOrEmail ?? "",
        })),
      });

      console.log("Loaded pds:", user.value);

    } catch (err) {
      console.error("Failed to load data:", err);
    } finally {
      isLoading.value = false;
    }
  }
  // ---------------------------------------------------------------------------------------
  onMounted(() => {
    loadLookups();
    loadData();
  });
  // ---------------------------------------------------------------------------------------

  let sortTimeout = null;

  // The Sorting Algorithm
  const sortEducationRecords = () => {
    if (sortTimeout) clearTimeout(sortTimeout);

    const levelWeight = {
      'ELEMENTARY': 1,
      'SECONDARY': 2,
      'VOCATIONAL / TRADE COURSE': 3,
      'COLLEGE': 4,
      'GRADUATE STUDIES': 5
    };

    // 1. Perform the Sort
    user.value.educationRecords.sort((a, b) => {
      const nameA = educationLevels.value.find(l => l.level_ID === a.educationLevelID)?.display_Name?.toUpperCase() || "";
      const nameB = educationLevels.value.find(l => l.level_ID === b.educationLevelID)?.display_Name?.toUpperCase() || "";

      const weightA = levelWeight[nameA] || 99;
      const weightB = levelWeight[nameB] || 99;

      if (weightA !== weightB) return weightA - weightB;

      const yearA = parseInt(a.attendanceFrom) || 0;
      const yearB = parseInt(b.attendanceFrom) || 0;
      return yearA - yearB;
    });

    // 2. CRITICAL FIX: Re-assign the OrderIndex based on the new sorted positions
    user.value.educationRecords.forEach((record, index) => {
      record.orderIndex = index + 1; // 1-based index (1, 2, 3...)
    });
  };

  // The Debounced Watcher
  watch(
    () => user.value.educationRecords,
    () => {
      if (sortTimeout) clearTimeout(sortTimeout);
      sortTimeout = setTimeout(() => {
        sortEducationRecords();
      }, 100);
    },
    { deep: true }
  );

  // Ref to access the hidden file input element (must match the ref attribute in your <input>)
  const requiredDocFile = ref(null);

  // Ref to handle the file input specifically for the passport photo
  const passportPhotoUpload = ref(null);

  // Holds the type of document currently being uploaded (e.g., 'PassportPhoto', 'Signature', 'Thumbmark')
  const documentTypeToUpload = ref('');


  // ------------------ Trigger Functions ------------------

  /**
   * Triggers the hidden file input for the Passport Photo.
   */
  function triggerPhotoUpload() {
    // Check for the reference you actually have in your HTML
    if (requiredDocFile.value) {
      // Tell the handler this is a photo before clicking
      documentTypeToUpload.value = 'PassportPhoto';
      requiredDocFile.value.click();
    } else {
      console.error("The hidden input 'requiredDocFile' was not found in the DOM.");
    }
  }

  /**
   * Triggers a generic hidden file input for other document types (Signature, Thumbmark).
   */
  function triggerGenericFileInput(docType) {
    documentTypeToUpload.value = docType;
    if (requiredDocFile.value) {
      requiredDocFile.value.click();
    } else {
      console.error("File input reference 'requiredDocFile' not found.");
    }
  }

  // -------------- DOCUMENT UPLOAD FUNCTIONS --------------
  /**
   * Handles file selection from the Passport Photo input.
   */
  async function handlePhotoUpload(event) {
    await handleFileUpload(event.target.files[0], 'PassportPhoto', event.target);
  }

  /**
   * Handles file selection from the generic input.
   */
  async function handleGenericFileSelection(event) {
    const file = event.target.files[0];
    if (!file) return;

    // The docType is set by triggerGenericFileInput before the click happens
    await handleFileUpload(file, documentTypeToUpload.value, event.target);
  }


  // ------------------ Centralized Upload Logic (The Core Fix) ------------------
  /**
   * Centralized File Upload Handler.
   * Sends the file to the POST /employee/uploadfile endpoint and saves the path.
   */
  async function handleFileUpload(file, docType, inputElement) {

    // 🐛 DEBUG STEP 1: Check the file object received by the handler
    console.log('DEBUG UPLOAD START: docType:', docType);
    console.log('DEBUG UPLOAD START: file.name:', file ? file.name : 'No file object');
    // -----------------------------------------------------------------------

    if (!file || !docType) {
      if (inputElement) inputElement.value = null; // Clear input
      return;
    }

    const formData = new FormData();
    // NOTE: Backend expects 'file' to be named IFormFile 'file' or the client sends IFormFile 'document'
    // I will assume the server-side IFormFile parameter name is 'file' as used in the C# signature:
    // public async Task<IActionResult> UploadPdsFile(IFormFile file)
    formData.append('file', file); // Use 'file' to match C# IFormFile parameter
    // formData.append('documentType', docType); // Removed, as the backend only accepts IFormFile 'file'
    // formData.append('personID', personId.value); // Removed, as the C# endpoint signature does not take additional params

    // Use the requested generic upload endpoint
    const endpoint = `/employee/uploadfile`;

    dialogTitle.value = "Uploading";
    isLoading.value = true;
    showDialog.value = true;

    try {
      // 1. Use POST for the new generic upload endpoint
      const res = await api.post(endpoint, formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });

      // 2. CRITICAL: Assuming the backend returns the unique file key:
      //    e.g., { fileKey: 'abcde12345.png' }
      // The C# code returns: return Ok(new { fileKey = fileKey })
      const fileKey = res.data.fileKey;
      const originalFileNameFromAPI = res.data.originalFileName;

      console.log(`1. API-Returned Filename: [${originalFileNameFromAPI}]`);

      // 3. Update the correct path/key in the user model (local state)
      switch (docType) {
        case 'PassportPhoto':
          // Use the key returned by the API (e.g., the filename or GUID)
          user.value.passportPhotoUrl = fileKey;
          user.value.passportPhotoOriginalName = file.name;
          console.log(`2. State Value after Assignment (Photo): [${user.value.passportPhotoOriginalName}]`);
          break;
        case 'BirthCertificate':
          // Save the key
          user.value.birthCertificateFileKey = fileKey;
          user.value.birthCertificateOriginalName = file.name;
          console.log(`2. State Value after Assignment (BC): [${user.value.birthCertificateOriginalName}]`);
          break;
        case 'MarriageCertificate':
          user.value.marriageCertificateFileKey = fileKey;
          user.value.marriageCertificateOriginalName = file.name;

          // ✅ FIX: Log the corrected property name
          console.log(`2. State Value after Assignment (MC): [${user.value.marriageCertificateOriginalName}]`);
          break;
      }

      dialogTitle.value = "Success";
      dialogMessage.value = `${file.name} uploaded successfully!`;
      // showDialog.value = true; (Already true)

    } catch (err) {
      console.error(`Failed to upload ${docType}:`, err);
      dialogTitle.value = "Error";
      if (err.response?.data?.message) {
        dialogMessage.value = err.response.data.message;
      } else {
        dialogMessage.value = `Failed to upload ${docType}. Please try again.`;
      }
      // showDialog.value = true; (Already true)
    } finally {
      isLoading.value = false;
      if (inputElement) inputElement.value = null; // Reset the file input value
      documentTypeToUpload.value = ''; // Clear document type
    }
  }

  function getFileUrl(fileKey) {
    if (!fileKey || !apiBaseUrl) {
      return null;
    }

    // Clean the API Base URL to handle potential trailing slashes for clean joining
    const apiBaseUrlClean = apiBaseUrl.endsWith('/') ? apiBaseUrl.slice(0, -1) : apiBaseUrl;

    // We rely on the /api/files/previewupload endpoint to stream ALL files (temp and permanent).
    // The 'fileKey' will contain either the permanent path OR the temporary GUID.
    // The C# backend will determine where to find the file based on the key's format.

    // The resulting URL will be:
    // https://localhost:5000/api/files/previewupload?fileKey=/PermanentPDSFiles/...
    // OR
    // https://localhost:5000/api/files/previewupload?fileKey=GUID.pdf

    return `${apiBaseUrlClean}/employee/previewupload?fileKey=${encodeURIComponent(fileKey)}`;
  }

  // ----------------------------------------------------------------------
  // 2. Main Function: Preview File
  // ----------------------------------------------------------------------
  async function previewFile(fileKey, title) {
    if (!fileKey) return;

    try {
      isLoading.value = true;

      const response = await api.get('/employee/previewupload', {
        params: { fileKey: fileKey },
        responseType: 'blob'
      });

      const contentType = response.headers['content-type'];
      const blob = new Blob([response.data], { type: contentType });
      const localUrl = URL.createObjectURL(blob);

      dialogTitle.value = `${title}`;

      // --- UPDATED STYLES FOR PORTRAIT FIX ---
      const iframeStyles = `<style>
      body {
        margin: 0;
        display: flex;
        justify-content: center;
        align-items: center;
        background-color: #f4f4f4;
        height: 100vh;
        overflow: hidden;
      }
      img {
        max-width: 100%;
        max-height: 100%;
        width: auto;
        height: auto;
        object-fit: contain;
      }
    </style>`;

      if (contentType.includes('image')) {
        dialogMessage.value = `
        <div style="height: 70vh; width: 100%; border: 1px solid #ccc; border-radius: 4px; overflow: hidden; background: #eee;">
            <iframe
              src="${localUrl}"
              width="100%"
              height="100%"
              style="border: none;"
              onload="this.contentDocument.head.innerHTML += '${iframeStyles.replace(/\n/g, '')}'"
            ></iframe>
        </div>
      `;
      } else {
        // PDF logic remains the same
        dialogMessage.value = `
        <div style="height: 70vh; width: 100%; border: 1px solid #ccc; border-radius: 4px; overflow: hidden;">
            <iframe src="${localUrl}" width="100%" height="100%" style="border: none;"></iframe>
        </div>
      `;
      }

      showDialog.value = true;
    } catch (error) {
      // ... error handling ...
    } finally {
      isLoading.value = false;
    }
  }

  const handleCloseDialog = () => {
    // If there's a blob URL in the message, release the memory
    if (dialogMessage.value.includes('blob:')) {
      const urlMatch = dialogMessage.value.match(/src="([^"]+)"/);
      if (urlMatch && urlMatch[1]) {
        URL.revokeObjectURL(urlMatch[1]);
      }
    }

    showDialog.value = false;
    dialogMessage.value = "";
  };

  // Add 'originalName' as a third parameter
  async function deleteFile(docType, fileKey, originalName) {
    if (!fileKey) return;

    const isPermanent = fileKey.includes('/PermanentPDSFiles/');
    let deleteEndpoint = '';
    let successMessage = '';
    const personId = user.value.personID;

    // Fallback if originalName wasn't passed or is missing
    const displayName = originalName || 'File';

    if (isPermanent && (docType === 'BirthCertificate' || docType === 'MarriageCertificate' || docType === 'PassportPhoto')) {
      deleteEndpoint = `/employee/cleardocumentkey?personId=${personId}&docType=${docType}`;
      // Use the filename in the success message
      successMessage = `${displayName} has been removed from your record.`;
    } else {
      deleteEndpoint = `/employee/deleteupload?fileKey=${fileKey}`;
      // Use the filename instead of the fileKey substring
      successMessage = `Temporary file '${displayName}' deleted successfully.`;
    }

    dialogTitle.value = "Deleting";
    isLoading.value = true;
    showDialog.value = true;

    const apiMethod = isPermanent ? api.post : api.delete;

    try {
      const res = await apiMethod(deleteEndpoint);

      if (res.status === 200) {
        clearLocalDocumentState(docType);
        dialogTitle.value = "Success";
        dialogMessage.value = successMessage;
      }
    } catch (err) {
      console.error(`Failed to delete/clear ${docType}:`, err);
      dialogTitle.value = "Error";
      dialogMessage.value = err.response?.data?.message || `Failed to delete ${displayName}.`;
    } finally {
      isLoading.value = false;
    }
  }

  /**
 * Clears the file key and original name from the local Vue state.
 */
  function clearLocalDocumentState(docType) {
    switch (docType) {
      case 'BirthCertificate':
        user.value.birthCertificateFileKey = null; // Change from '' to null
        user.value.birthCertificateOriginalName = null; // Change from '' to null
        break;
      case 'MarriageCertificate':
        user.value.marriageCertificateFileKey = null;
        user.value.marriageCertificateOriginalName = null;
        break;
      case 'PassportPhoto':
        user.value.passportPhotoUrl = null;
        user.value.passportPhotoOriginalName = null;
        break;
    }
  }

  // ------------------ Save Methods ------------------
  async function save() {

    if (sortTimeout) clearTimeout(sortTimeout);
    sortEducationRecords();

    // 1. Reset states (Optional but good practice)
    showValidationErrors.value = true;

    // 2. Perform Validation FIRST
    const validation = validatePDS();
    if (!validation.isValid) {
      showValidationErrors.value = true; // Highlights the inputs
      dialogTitle.value = "Required Fields Missing";
      dialogMessage.value = validation.message;
      showDialog.value = true;
      return; // Stop the save process
    }

    // 3. Validation passed, now start loading
    dialogTitle.value = "Saving";
    isLoading.value = true;

    try {
      // Always enforce personID + email from token
      user.value.personID = personId.value;
      user.value.email = personEmail.value;

      // -----------------------------------------------------------------------
      // 🛑 CRITICAL FIX: Normalization Helpers (Defined Here for Scope)
      // -----------------------------------------------------------------------
      const normalizeString = (val) => {
        if (typeof val !== 'string') return val ?? null;
        return val.trim() === '' ? null : val;
      };

      const normalizeDate = (val) => {
        if (!val || val === "" || val === undefined) return null;
        // Ensure it's a valid date string (YYYY-MM-DD)
        const d = new Date(val);
        return isNaN(d.getTime()) ? null : val;
      };

      const normalizeNumber = (val) => val === 0 || val === '' || val === null ? null : val;

      // ⭐ AGGRESSIVE BOOLEAN FIX: Forces all non-true values to false (fixes C# deserialization errors) ⭐
      const normalizeBoolean = (val) => {
        // Explicitly check for true values (true, string 'true', or 1)
        if (val === true || val === 'true' || val === 1) return true;

        // For ALL other values (false, 0, null, undefined, ""), return false.
        return false;
      };

      // 🔍 DEBUG LOG 1: Check raw data from the UI table before processing
      console.log('RAW Education Records from UI:', JSON.parse(JSON.stringify(user.value.educationRecords)));

      const formattedRecords = user.value.educationRecords = (user.value.educationRecords ?? []).map(e => {
        // Use the ID from your dropdown to find the object in the lookup array
        const levelLookup = educationLevels.value.find(l => l.level_ID === e.educationLevelID);

        return {
          educationID: e.educationID ?? 0,
          personID: user.value.personID,
          level: levelLookup ? levelLookup.display_Name : null, // The string Swagger wants
          schoolName: normalizeString(e.schoolName),
          degree: normalizeString(e.degree),
          attendanceFrom: normalizeString(e.attendanceFrom),
          attendanceTo: normalizeString(e.attendanceTo),
          highestLevel: normalizeString(e.highestLevelUnits),
          yearGraduated: normalizeString(e.yearGraduated),
          honors: normalizeString(e.scholarshipAcademicHonors),

          orderIndex: e.orderIndex
        };
      }).filter(e => e.level !== null);

      // 🔍 DEBUG LOG 2: Check what we are actually sending to the API
      console.log('FINAL Education Records for API payload:', user.value.educationRecords);

      // Applying robust normalization to the other collections
      user.value.children = (user.value.children ?? []).map(c => ({
        // Ensure these property names match your C# ChildDto exactly
        fullName: normalizeString(c.fullName || c.name), // Use fullName as seen in your API loop
        dateOfBirth: normalizeDate(c.dateOfBirth)
      })).filter(c => c.fullName !== null);

      user.value.voluntaryWorks = (user.value.voluntaryWorks ?? []).map(v => ({
        ...v,
        organizationName: normalizeString(v.organizationName),
        position: normalizeString(v.position),
        dateFrom: normalizeDate(v.dateFrom),
        dateTo: normalizeDate(v.dateTo),
        numberOfHours: normalizeNumber(v.numberOfHours)
      }));

      user.value.trainings = (user.value.trainings ?? []).map(t => ({
        ...t,
        title: normalizeString(t.title),
        sponsor: normalizeString(t.sponsor),
        dateFrom: normalizeDate(t.dateFrom),
        dateTo: normalizeDate(t.dateTo),
        numberOfHours: normalizeNumber(t.numberOfHours)
      }));

      user.value.civilServiceEligibilities = (user.value.civilServiceEligibilities ?? []).map(c => ({
        ...c,
        eligibility: normalizeString(c.eligibility),
        rating: normalizeNumber(c.rating),
        dateOfExamination: normalizeDate(c.dateOfExamination),
        dateReleased: normalizeDate(c.dateReleased)
      }));

      user.value.workExperiences = (user.value.workExperiences ?? []).map(w => ({
        ...w,
        positionTitle: normalizeString(w.positionTitle),
        department: normalizeString(w.department),
        salary: normalizeNumber(w.salary),
        dateFrom: normalizeDate(w.dateFrom),
        dateTo: normalizeDate(w.dateTo)
      }));

      user.value.skillsHobbies = (user.value.skillsHobbies ?? []).map(s => ({
        ...s,
        skillOrHobby: normalizeString(s.skillOrHobby)
      }));

      user.value.distinctions = (user.value.distinctions ?? []).map(d => ({
        ...d,
        distinction: normalizeString(d.distinction)
      }));

      user.value.memberships = (user.value.memberships ?? []).map(m => ({
        ...m,
        organizationName: normalizeString(m.organizationName)
      }));

      // ✅ PDS Section IV - References Collection
      user.value.references = (user.value.references ?? []).map(r => ({
        ...r,
        name: normalizeString(r.name),
        officeOrResidentialAddress: normalizeString(r.address),
        contactNoOrEmail: normalizeString(r.telephoneNo),
      }));

      // Filter out any empty reference rows
      user.value.references = user.value.references.filter(r =>
        r.name !== null || r.address !== null || r.telephoneNo !== null
      );

      // 🛑 REMOVED: The obsolete 'if (user.value.declaration)' block that used 'c4.value'

      // -------------------------------------------------------------------------
      // 4. Create and Attach PDS Section IV Object (APPLIED AGGRESSIVE BOOLEAN NORMALIZATION)
      // -------------------------------------------------------------------------
      user.value.pdsSectionIV = {
        // Q. 34: Relatives
        Q34a_RelatedWithin3rd: normalizeBoolean(user.value.areThirdDegree),
        Q34b_RelatedWithin4th: normalizeBoolean(user.value.areFourthDegree),
        Q34_Details: normalizeString(user.value.thirdFourthDegreeDetails),

        // Q. 35: Admin/Criminal Charges
        Q35a_AdminOffense: normalizeBoolean(user.value.guiltyAdministrativeOffense),
        Q35b_CriminallyCharged: normalizeBoolean(user.value.criminallyCharged),
        Q35a_AdminDetails: normalizeString(user.value.adminOffenseDetails),
        Q35b_CriminalDetails: normalizeString(user.value.criminalChargeDetails),

        // Q35 fields from C# model
        Q35b_DateFiled: normalizeDate(user.value.q35bDateFiled),
        Q35b_Status: normalizeString(user.value.q35bStatus),
        HasPendingCase: normalizeBoolean(user.value.hasPendingCase),
        PendingCaseDetails: normalizeString(user.value.pendingCaseDetails),

        // Q. 36: Convicted of Crime
        Q36_Convicted: normalizeBoolean(user.value.convictedOfCrime),
        Q36_Details: normalizeString(user.value.convictedCrimeDetails),

        // Q36 fields from C# model
        ConvictedFinalJudgment: normalizeBoolean(user.value.convictedFinalJudgment),
        ConvictedFinalDetails: normalizeString(user.value.convictedFinalDetails),

        // Q. 37: Separation from Service
        Q37_Separated: normalizeBoolean(user.value.separatedFromService),
        Q37_Details: normalizeString(user.value.separatedServiceDetails),

        // Q. 37 fields from C# model
        Q38b_ResignedForCampaign: normalizeBoolean(user.value.resignedFromGovt),
        Q38b_Details: normalizeString(user.value.resignedGovtDetails),

        // Q. 38: Election History
        Q38a_Candidate: normalizeBoolean(user.value.candidateInElection),
        Q38a_Details: normalizeString(user.value.candidateElectionDetails),

        // Q. 39: Immigrant/Dual Citizenship
        Q39_Country: normalizeBoolean(user.value.immigrantOrResident),
        Q39_Details_Country: normalizeString(user.value.immigrantResidentDetails),

        // Q. 40: Other Information
        Q40a_IndigenousGroup: normalizeBoolean(user.value.indigenousGroup),
        Q40a_Details: normalizeString(user.value.indigenousGroupDetails),
        Q40b_Disability: normalizeBoolean(user.value.personWithDisability),
        Q40b_Details_IDNo: normalizeString(user.value.pwdIDNo),
        Q40c_SoloParent: normalizeBoolean(user.value.soloParent),
        Q40c_Details_IDNo: normalizeString(user.value.soloParentIDNo),
      };

      user.value.Declaration = {
        DeclarationID: user.value.declarationID,
        GovernmentIssuedID: normalizeString(user.value.govIdType), // ✅ FIX APPLIED: Maps UI field to correct DTO property
        LicensePassportIDNo: normalizeString(user.value.govIdNo),
        DateOfIssuance: normalizeDate(user.value.idIssueDate),
        PlaceOfIssuance: normalizeString(user.value.idIssuePlace),
        DateAccomplished: normalizeDate(user.value.dateAccomplished),

        PhotoFileKey: normalizeString(user.value.passportPhotoUrl),
        PhotoOriginalName: normalizeString(user.value.passportPhotoOriginalName),
        SignatureFileKey: normalizeString(user.value.signatureFileKey),
        RightThumbmarkFileKey: normalizeString(user.value.rightThumbmarkFileKey),
        DateSchedule: normalizeDate(user.value.dateSchedule),

        // Note: SubscribedDate and NotaryName are generally part of the PDS Section IV/top-level model,
        // but were not explicitly listed in your PdsSectionVDeclarationDto.
        // For now, we will assume they are not needed in this specific DTO for the backend.
      };
      // -------------------------------------------------------------------------

      // -------------------------------------------------------------------------
      // 5. Cleanup Top-Level PDS 4 Fields (Setting to null to prevent API from seeing duplicate keys)
      // -------------------------------------------------------------------------
      user.value.areThirdDegree = null;
      user.value.areFourthDegree = null;
      user.value.guiltyAdministrativeOffense = null;
      user.value.criminallyCharged = null;
      user.value.convictedOfCrime = null;
      user.value.separatedFromService = null;
      user.value.candidateInElection = null;
      user.value.resignedFromGovt = null;
      user.value.immigrantOrResident = null;
      user.value.indigenousGroup = null;
      user.value.personWithDisability = null;
      user.value.soloParent = null;

      user.value.thirdFourthDegreeDetails = null;
      user.value.adminOffenseDetails = null;
      user.value.criminalChargeDetails = null;
      user.value.convictedCrimeDetails = null;
      user.value.separatedServiceDetails = null;
      user.value.candidateElectionDetails = null;
      user.value.resignedGovtDetails = null;
      user.value.immigrantResidentDetails = null;
      user.value.indigenousGroupDetails = null;
      user.value.pwdIDNo = null;
      user.value.soloParentIDNo = null;

      // Also cleanup Q42 fields
      user.value.signature_text = null;
      user.value.dateAccomplished = null;
      user.value.subscribedDate = null;
      user.value.notary_name = null;
      user.value.passportPhotoUrl = null;
      //user.value.passportPhotoOriginalName = null;
      // -------------------------------------------------------------------------
      // 6. Apply robust global cleanup for remaining top-level individual fields
      const fileKeyProperties = [
        'passportPhotoUrl', // Declaration Photo
        'signatureFileKey', // Declaration Signature
        'rightThumbmarkFileKey', // Declaration Thumbmark
        'birthCertificateFileKey', // ✅ FIX 2: Use the new property name
        'marriageCertificateFileKey', // ✅ FIX 2: Use the new property name

        //'birthCertificateOriginalName',
        //'marriageCertificateOriginalName',
        //'passportPhotoOriginalName'
      ];

      // Safer Cleanup: Only nullify if it's strictly an empty string or invalid number
      Object.keys(user.value).forEach(key => {
        // 1. Skip objects (like Declaration, pdsSectionIV) and arrays (children, etc.)
        if (typeof user.value[key] === 'object' && user.value[key] !== null) return;

        // 2. Skip protected keys (like your file keys)
        if (fileKeyProperties.includes(key)) return;

        // 3. Only nullify actual empty strings
        if (typeof user.value[key] === 'string' && user.value[key].trim() === '') {
          user.value[key] = null;
        }

        // 4. Handle numeric fields safely
        const numericFields = ['heightCM', 'weightKG', 'civilStatusID', 'sexID', 'departmentID'];
        if (numericFields.includes(key) && (user.value[key] === 0 || user.value[key] === '')) {
          user.value[key] = null;
        }
      });

      console.log('Final DTO Check: Birth Certificate Original Name:', user.value.birthCertificateOriginalName);
      console.log('Final DTO Check: Marriage Certificate Original Name:', user.value.marriageCertificateOriginalName);
      console.log('Final DTO Check: Passport Photo Original Name:', user.value.passportPhotoOriginalName);

      // API Call
      await api.put(`/employee/user/${user.value.personID}`, user.value, {
        headers: {
          'Content-Type': 'application/json'
        }
      });

      dialogTitle.value = "Success";
      dialogMessage.value = "User Information Update Successful!";
      showDialog.value = true;

      // Reload data after a successful save to update the UI
      await loadData();

      console.log('Post-Load Check: Passport Photo Original Name:', user.value.passportPhotoOriginalName);

    } catch (err) {
      console.error("Failed to save user information:", err);
      dialogTitle.value = "Error";
      //1. Prioritize validation errors (if present)
      if (err.response?.data?.errors) {
        dialogMessage.value = Object.values(err.response.data.errors).flat().join("\n");
        //2. Use API message
      } else if (err.response?.data?.message) {
        dialogMessage.value = err.response.data.message; // Use API message
        //3. Fallback to generic message
      } else {
        dialogMessage.value = "Failed to save user information.";
      }
      showDialog.value = true;
    } finally {
      isLoading.value = false;
    }
  }

  async function downloadPDS() {
    // 1. Get the employee ID from the reactive user object
    const employeeId = user.value.personID;

    if (!employeeId) {
      // This dialog logic should match the rest of your app's error handling
      alert("Error: Employee ID is missing. Cannot generate PDF.");
      return;
    }

    // Set loading state (if you have one)
    isLoading.value = true;

    try {
      // 2. Make an API call to the PDF generation endpoint
      // IMPORTANT: Use the /pdf/ endpoint, which corresponds to the controller action
      // that uses the GeneratePdsPdf method.
      const response = await api.get(`/employee/pdf/${employeeId}`, {
        // CRITICAL: responseType must be 'blob' for binary file download
        responseType: 'blob'
      });

      // 3. Create a temporary URL for the downloaded file (blob)
      const url = window.URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));

      // 4. Create a hidden link element and click it to trigger the download
      const link = document.createElement('a');
      link.href = url;

      // Set the suggested filename. This should match the one set in the C# controller
      // for consistency, but if the controller sets the filename, the browser should use it.
      // We default to a client-side name as a fallback:
      const fileName = `PDS_${user.value.surname}_${user.value.firstName}.pdf`;
      link.setAttribute('download', fileName);

      document.body.appendChild(link);
      link.click();

      // 5. Clean up the temporary URL and link element
      window.URL.revokeObjectURL(url);
      document.body.removeChild(link);

      console.log("PDF download initiated successfully.");

    } catch (error) {
      console.error("Error downloading PDS PDF:", error);
      // Display a user-friendly error message
      alert("Failed to download the PDF. Please check your connection and ensure all data is saved.");
    } finally {
      // Clear loading state
      isLoading.value = false;
    }
  }

  // ✅ Delete voluntary work
  function deleteVoluntaryWork(index) {
    user.value.voluntaryWorks.splice(index, 1);
  }
  // ✅ Delete training
  function deleteTraining(index) {
    user.value.trainings.splice(index, 1);
  }
  // ✅ Delete skillsHobby
  function deleteSkillOrHobby(index) {
    user.value.skillsHobbies.splice(index, 1);
  }
  // ✅ Delete distinction
  function deleteDistinction(index) {
    user.value.distinctions.splice(index, 1);
  }
  // ✅ Delete membership
  function deleteMemberShip(index) {
    user.value.memberships.splice(index, 1);
  }
  // ✅ Delete civil service eligibility
  function deleteCivilServiceEligibility(index) {
    user.value.civilServiceEligibilities.splice(index, 1);
  }
  // ✅ Delete work experience
  function deleteWorkExperience(index) {
    user.value.workExperiences.splice(index, 1);
  }

  // --- Watchers for PDS 4 (FIXED: Using user.value) ---

  // Q34: Are you related by consanguinity or affinity?
  watch(() => [user.value.areThirdDegree, user.value.areFourthDegree], ([newThird, newFourth]) => {
    // Clear only when BOTH are false (NO)
    if (newThird === false && newFourth === false) {
      user.value.thirdFourthDegreeDetails = null;
    }
  }, { deep: true });

  // Q35a: Guilty of administrative offense?
  watch(() => user.value.guiltyAdministrativeOffense, (newValue) => {
    // If the user selects NO (false), clear the details field
    if (newValue === false) {
      user.value.adminOffenseDetails = null;
    }
  }, { immediate: true });

  // Q35b: Criminally charged? (Clears three fields)
  watch(() => user.value.criminallyCharged, (newValue) => {
    if (newValue === false) {
      user.value.criminalChargeDetails = null;
      user.value.q35bDateFiled = null;
      user.value.q35bStatus = null;
    }
  });

  // Q36: Convicted of crime?
  watch(() => user.value.convictedOfCrime, (newValue) => {
    if (newValue === false) {
      user.value.convictedCrimeDetails = null;
    }
  });

  // Q37: Separated from service?
  watch(() => user.value.separatedFromService, (newValue) => {
    if (newValue === false) {
      user.value.separatedServiceDetails = null;
    }
  });

  // Q38a: Candidate in election?
  watch(() => user.value.candidateInElection, (newValue) => {
    if (newValue === false) {
      user.value.candidateElectionDetails = null;
    }
  });

  // Q38b: Resigned from government?
  watch(() => user.value.resignedFromGovt, (newValue) => {
    if (newValue === false) {
      user.value.resignedGovtDetails = null;
    }
  });

  // Q39: Immigrant or permanent resident?
  watch(() => user.value.immigrantOrResident, (newValue) => {
    if (newValue === false) {
      user.value.immigrantResidentDetails = null;
    }
  });

  // Q40a: Indigenous group?
  watch(() => user.value.indigenousGroup, (newValue) => {
    if (newValue === false) {
      user.value.indigenousGroupDetails = null;
    }
  });

  // Q40b: Person with disability?
  watch(() => user.value.personWithDisability, (newValue) => {
    if (newValue === false) {
      user.value.pwdIDNo = null;
    }
  });

  // Q40c: Solo parent?
  watch(() => user.value.soloParent, (newValue) => {
    if (newValue === false) {
      user.value.soloParentIDNo = null;
    }
  });</script>

<style scoped>
  /* ************************************************************************** */
  /* 1. DASHBOARD GRID LAYOUT STYLES (MATCHING EmployeeID.vue) */
  /* ************************************************************************** */
  /* Renamed .app-grid-layout to .app-layout and updated style */


  /*SCONTACT*/
  .form-section {
    background: #fff;
    border-radius: 12px;
    padding: 24px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    margin-bottom: 24px;
  }

  .section-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 24px;
    padding-bottom: 12px;
    border-bottom: 2px solid #e5e7eb;
  }

  .section-icon {
    width: 32px;
    height: 32px;
    background: linear-gradient(135deg, #3b82f6, #1d4ed8);
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 16px;
    font-weight: 600;
  }

  .section-title {
    margin: 0;
    font-size: 18px;
    font-weight: 700;
    color: #1f2937;
  }

  .form-row {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 16px;
  }

    .form-row:last-child {
      margin-bottom: 0;
    }

    .form-row span {
      min-width: 140px;
      font-weight: 500;
      color: #374151;
      font-size: 14px;
    }

  .auth-input,
  .auth-select,
  .phone-flag {
    padding: 12px 16px;
    border: 1px solid #d1d5db;
    border-radius: 8px;
    font-size: 14px;
    background-color: #f9fafb;
    transition: all 0.2s ease;
    height: 48px;
    box-sizing: border-box;
  }

  .auth-input {
    flex: 1;
  }

  .auth-select {
    flex: 1;
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
    background-color: #f9fafb;
    background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
    background-position: right 8px center;
    background-repeat: no-repeat;
    background-size: 16px;
    cursor: pointer;
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

  .two-column-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;
  }

  .full-width {
    grid-column: 1 / -1;
  }

  .required::after {
    content: " *";
    color: #ef4444;
  }
  /*FINISH*/

  /* Select Dropdown Arrow */
  .auth-select {
    cursor: pointer;
    appearance: none;
    background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
    background-position: right 12px center;
    background-repeat: no-repeat;
    background-size: 16px;
    padding-right: 40px;
  }

  /* Responsive */
  @media (max-width: 768px) {
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
    box-shadow: 0 4px 12px rgba(0,0,0,0.05);
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

    .pds-table th, .pds-table td {
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

  input[type="radio"], input[type="checkbox"] {
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

  /* ************************************************************************** */
  /* 4. Form Elements and Labels (UPDATED COLORS) */
  /* ************************************************************************** */
  .auth-input-label-bold {
    font-size: 0.8rem;
    font-weight: 700;
    color: #4a5568;
    text-transform: uppercase;
    margin-bottom: 6px;
    display: block;
  }

  .auth-input-label {
    display: block;
    font-size: 0.85rem;
    font-weight: 500;
    color: #333; /* Updated color */
    margin-bottom: 5px;
  }

  .auth-input,
  select.auth-input {
    width: 100%;
    padding: 8px 12px;
    border: 1px solid #ced4da; /* Matched input border */
    border-radius: 6px; /* Slightly larger border radius */
    font-size: 1rem;
    box-sizing: border-box;
    transition: border-color 0.2s;
    background-color: #ffffff;
    color: #1a202c;
  }

    .auth-input:focus {
      outline: none;
      border-color: #06195e;
      background-color: white;
      box-shadow: 0 0 0 3px rgba(6, 25, 94, 0.1);
    }

    .auth-input:disabled {
      background-color: #edf2f7;
      cursor: not-allowed;
      color: #718096;
    }


  /* Style for the Address checkbox/label alignment (retained/updated color) */
  span.auth-input-label-bold {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    color: #333;
  }

  label.auth-input-label-bold {
    display: flex;
    align-items: center;
    margin-left: 20px !important;
    font-weight: 500;
    color: #333;
  }

  input[type="checkbox"] {
    margin-right: 5px;
    width: auto;
    accent-color: #007bff; /* Primary blue for checkboxes */
  }

  /* --- Validation Styles --- */
  .input-error {
    border: 2px solid #e53e3e !important;
    background-color: #fff5f5 !important;
  }

    .input-error::placeholder {
      color: #dc3545;
      opacity: 0.5;
    }

  @keyframes shake {
    0% {
      transform: translateX(0);
    }

    25% {
      transform: translateX(3px);
    }

    75% {
      transform: translateX(-3px);
    }

    100% {
      transform: translateX(0);
    }
  }

  /* Ensure table inputs don't look too cramped with the borders */
  .pds-table input.auth-input {
    transition: all 0.2s ease;
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

  /* File Action Containers (retained) */
  .file-action-container {
    display: inline-flex; /* Changed from flex to inline-flex to fit content only */
    align-items: center; /* Center preview and text vertically */
    gap: 15px;
    background: #f9f9f9;
    padding: 10px 15px; /* Tightened vertical padding */
    border-radius: 8px;
    border: 1px solid #ddd;
    max-width: 100%; /* Ensures it doesn't break layout */
  }

  .photo-preview-box {
    width: 70px; /* Slightly smaller to keep the container compact */
    height: 70px;
    border: 1px solid #06195e;
    border-radius: 4px;
    overflow: hidden;
    flex-shrink: 0;
  }

    .photo-preview-box img {
      width: 100%;
      height: 100%;
      object-fit: cover; /* Ensures the face isn't stretched */
    }

  .file-info-group {
    display: flex;
    flex-direction: column;
    justify-content: center; /* Centers items vertically if only one is present */
    gap: 6px; /* Tight gap between name and button */
    min-width: 0;
  }

  .uploaded-file-details {
    display: flex;
    flex-direction: column;
    gap: 4px;
    min-width: 0;
  }

  .file-name-label {
    font-size: 0.75rem; /* Slightly smaller for a cleaner look */
    color: #06195e; /* Match Makati blue for better branding */
    font-weight: 700;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 180px; /* Keeps the filename from pushing the box too wide */
  }

  .file-name-display {
    font-size: 0.9rem;
    color: #334155;
    font-weight: 500;
    max-width: 150px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .action-buttons {
    display: flex;
    gap: 6px;
  }

  /* Unified Button Style for Upload Section */
  .upload-file-btn,
  .preview-file-btn,
  .delete-file-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    padding: 0 15px; /* Side padding only */
    border-radius: 8px;
    font-weight: 700;
    font-size: 0.85rem;
    cursor: pointer;
    transition: all 0.2s ease;
    border: 1px solid;
    height: 38px; /* Fixed height is key for alignment */
    box-sizing: border-box;
    white-space: nowrap;
  }

    .upload-file-btn:hover {
      background: #fef9c3;
      border-color: #ecc94b;
      transform: translateY(-1px);
    }

    .upload-file-btn img {
      width: 18px;
      height: auto;
      /* Ensures the dark blue icon stays sharp */
      display: block;
    }

  .preview-file-btn {
    background: #eff6ff;
    color: #2563eb;
    border-color: #bfdbfe;
  }

    .preview-file-btn:hover {
      background: #2563eb;
      color: white;
      border-color: #2563eb;
      transform: translateY(-1px);
    }

  .delete-file-btn {
    background: #fff5f5;
    color: #c53030;
    border-color: #feb2b2;
  }

    .delete-file-btn:hover {
      background: #c53030;
      color: white;
      border-color: #c53030;
      transform: translateY(-1px);
    }

    /* Icon Consistency */
    .preview-file-btn img,
    .delete-file-btn img {
      width: 16px;
      height: 16px;
      filter: none; /* Let the brightness filter on hover handle the color swap */
    }

    .preview-file-btn:hover img,
    .delete-file-btn:hover img {
      filter: brightness(0) invert(1);
    }

  .cancel-btn, .submit-btn {
    width: 100%; /* Allows flex-grow to work properly */
    display: flex;
    justify-content: center;
    align-items: center;
  }

  /* Container to align tabs left and appointment box right */
  .pds-top-bar {
    display: flex;
    justify-content: space-between;
    align-items: flex-end; /* Aligns with the bottom of the tabs */
    padding: 0 5px;
    margin-top: 10px; /* Give it some breathing room from the title */
    flex-shrink: 0;
  }

  /* The Small Appointment Box */
  .appointment-summary-card {
    background: white;
    padding: 20px 25px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    gap: 20px;
    margin-bottom: 30px;
    cursor: pointer;
    box-shadow: 0 4px 6px rgba(0,0,0,0.05);
    border-left: 6px solid #f8d745; /* Makati Yellow Accent */
    transition: transform 0.2s ease;
  }

    .appointment-summary-card:hover {
      transform: translateY(-3px);
      box-shadow: 0 8px 15px rgba(0,0,0,0.1);
    }

  .appointment-icon {
    font-size: 1.8rem;
    color: #06195e;
    background: #f0f4f8;
    width: 50px;
    height: 50px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 10px;
  }

  .appointment-label {
    font-size: 0.85rem;
    color: #718096;
    font-weight: 700;
    text-transform: uppercase;
    margin: 0;
  }

  .appointment-value {
    font-size: 1rem;
    font-weight: 700;
    color: #2d3748;
    margin: 4px 0 0 0;
  }

    .appointment-value.empty {
      color: #dc3545;
    }

  /* Status Badges */
  .status-badge {
    font-size: 0.7rem;
    padding: 2px 8px;
    border-radius: 12px;
    text-transform: capitalize;
  }

    .status-badge.approved {
      background: #d4edda;
      color: #155724;
    }

    .status-badge.pending {
      background: #fff3cd;
      color: #856404;
    }

  .arrow-icon {
    margin-left: auto;
    color: #ced4da;
  }

  .appointment-modal-content {
    text-align: left;
    padding: 10px 5px;
  }

  .button-group-row {
    display: flex;
    gap: 15px;
    margin-top: 25px;
    position: sticky;
    bottom: -20px; /* Aligns with the padding of the wrapper */
    background: white;
    padding: 15px 0;
    border-top: 1px solid #eee;
    z-index: 10;
  }

  .cancel-btn {
    background: #6c757d !important; /* Gray for secondary action */
    flex: 1;
  }

  .submit-btn {
    background: #06195e;
    color: white;
    padding: 12px 25px;
    border-radius: 8px;
    font-weight: 700;
    border: none;
    cursor: pointer;
  }

  /* Ensure the date input looks consistent with your auth inputs */
  input[type="date"].auth-input {
    cursor: pointer;
    appearance: none;
    -webkit-appearance: none;
  }

  /* ************************************************************************** */
  /* 6. Notes/Instructions Block (UPDATED COLOR SCHEME) */
  /* ************************************************************************** */
  .note {
    background: #fffaf0;
    border-left: 4px solid #ecc94b;
    padding: 15px;
    margin: 10px 0;
    border-radius: 0 8px 8px 0;
    font-size: 0.85rem;
    color: #744210;
    line-height: 1.5;
  }

    .note span {
      display: block;
    }

  textarea.auth-input {
    resize: vertical;
    min-height: 80px;
    line-height: 1.5;
    font-family: inherit;
  }


  /* ************************************************************************** */
  /* 7. Transition Styles (Retained/Updated) */
  /* ************************************************************************** */
  .fade-slide-enter-active,
  .fade-slide-leave-active {
    transition: opacity 0.3s ease, transform 0.3s ease;
  }

  .fade-slide-enter-from {
    opacity: 0;
    transform: translateY(10px); /* Changed from X to Y to match EmployeeID */
  }

  .fade-slide-leave-to {
    opacity: 0;
    transform: translateY(-10px); /* Changed from X to Y to match EmployeeID */
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

    .leftMenu, .header, .dashboard-content {
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
