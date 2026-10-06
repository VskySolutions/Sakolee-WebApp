<template>
  <app-form-dialog v-model="dialogOpen" :title="studentName" :subtitle="studentSubtitle" :avatar-text="studentInitials" size="lg" hide-save hide-footer>
    <div v-if="loading" class="view-student__loading">
      <q-spinner color="primary" size="32px" />
    </div>

    <div v-else class="view-student-scss">
      <!-- =====================================================
           SUMMARY
           ===================================================== -->
      <div class="student-summary row q-col-gutter-md">
        <div class="col-12 col-sm-6 col-md-3">
          <div class="summary-card">
            <div class="summary-card__label">STATUS</div>

            <q-badge :class="form.active ? 'status-badge active' : 'status-badge inactive'">
              {{ form.active ? "Active" : "Inactive" }}
            </q-badge>
          </div>
        </div>

        <div class="col-12 col-sm-6 col-md-3">
          <div class="summary-card">
            <div class="summary-card__label">AGE & GENDER</div>

            <div class="summary-card__value">
              {{ studentAge }} yrs • {{ form.gender || "—" }}
            </div>
          </div>
        </div>

        <div class="col-12 col-sm-6 col-md-3">
          <div class="summary-card">
            <div class="summary-card__label">LOCATION</div>

            <div class="summary-card__value summary-card__value--primary">
              {{ form.location || "—" }}
            </div>
          </div>
        </div>

        <div class="col-12 col-sm-6 col-md-3">
          <div class="summary-card">
            <div class="summary-card__label">WEEKLY HOURS</div>

            <div class="summary-card__value font-mono">
              {{ form.weeklyHours || "00h 00m" }}
            </div>
          </div>
        </div>
      </div>

      <!-- =====================================================
           STUDENT INFORMATION + FAMILY
           ===================================================== -->
      <div class="row q-col-gutter-md">
        <!-- Student Information -->
        <div class="col-12 col-md-6">
          <section class="border-80 br-16 pa-20">
            <div class="info-card__header">
              <q-icon name="o_person_outline" class="fs-16 text-4d" />
              <span class="text-4d fw-700 fs-12 lh-16">STUDENT INFORMATION</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Full Name</span>
                <span class="fs-11 fw-600 text-2e">{{ studentName }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Birth Date</span>
                <span class="fs-11 fw-600 text-2e">{{ formatDate(form.birthDate) }}</span>
              </div>

              <!-- <div class="info-row">
                <span class="fs-11 fw-400 text-86">School Grade</span>
                <span class="fs-11 fw-600 text-2e">{{ form.gradeLevel || "—" }}</span>
              </div> -->

              <div class="info-row">
  <span class="fs-11 fw-400 text-86">School Grade</span>
  <span class="fs-11 fw-600 text-2e">{{ displayGradeLevel }}</span>
</div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Apparel Size</span>
                <span class="fs-11 fw-600 text-2e">{{ form.tShirtSize || "—" }}</span>
              </div>
            </div>
          </section>
        </div>

        <!-- Linked Family Account -->
        <div class="col-12 col-md-6">
          <section class="border-80 br-16 pa-20">
            <div class="info-card__header">
              <!-- <q-icon name="o_groups" class="fs-16 text-4d" /> -->
              <span class="material-symbols-outlined fs-16">family_restroom</span>
              <span class="text-4d fw-700 fs-12 lh-16">LINKED FAMILY ACCOUNT</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Family Name</span>
                <span class="fs-11 fw-600 text-d4">
                  {{ form.familyName || "—" }}
                </span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Primary Contact</span>
                <span class="fs-11 fw-600 text-2ee">
                  {{ form.primaryContact || "—" }}
                </span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Contact Phone</span>
                <span class="fs-11 fw-600 text-2e">
                  {{ form.cellPhone || "—" }}
                </span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Emergency</span>
                <span class="fs-11 fw-600 text-1a">
                  {{ form.emergencyContact || "—" }}
                </span>
              </div>
            </div>
          </section>
        </div>
      </div>

      <!-- =====================================================
           ENROLLED CLASSES
           ===================================================== -->
      <section class="border-80 br-16 pa-20 mt-24">
        <div class="info-card__header">
          <q-icon name="o_school" class="fs-16 text-4d" />
          <span class="text-4d fw-700 fs-12 lh-16">ENROLLED CLASSES ({{ form.classes.length }})</span>
        </div>

        <div class="classes-list mt-12">
          <div v-if="!form.classes.length" class="pa-20 fs-11 text-86 text-center">
            No enrolled classes.
          </div>

          <div v-for="item in form.classes" :key="item.id" class="class-row">
            <div class="class-row__content">
              <div class="class-row__name">
                {{ item.name }}
              </div>

              <div class="class-row__schedule">
                {{ item.schedule || "—" }}
              </div>
            </div>

            <q-badge class="class-row__badge">
              {{ item.active === false ? "Class Inactive" : "Enrolled" }}
            </q-badge>
          </div>
        </div>
      </section>

      <!-- =====================================================
           ADDITIONAL DETAILS HIDDEN____________________________________________> HIDDEN
           ===================================================== -->
      <div class="additional-details hidden">
        <!-- Identity -->
        <section class="detail-section">
          <div class="detail-section__title">
            <q-icon name="o_badge" />
            <span>IDENTITY</span>
          </div>

          <div class="row q-col-gutter-md">
            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Student Number</div>
                <div class="detail-item__value">{{ form.studentNumber || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Gender</div>
                <div class="detail-item__value">{{ form.gender || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Admission Date</div>
                <div class="detail-item__value">{{ formatDate(form.admissionDate) }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Allow Text Messaging</div>
                <div class="detail-item__value">
                  {{ form.allowTextMessaging ? "Yes" : "No" }}
                </div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Email</div>
                <div class="detail-item__value">{{ form.email || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Cell Phone</div>
                <div class="detail-item__value">{{ form.cellPhone || "—" }}</div>
              </div>
            </div>
          </div>
        </section>

        <!-- School -->
        <section class="detail-section">
          <div class="detail-section__title">
            <q-icon name="o_school" />
            <span>SCHOOL</span>
          </div>

          <div class="row q-col-gutter-md">
            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">School</div>
                <div class="detail-item__value">{{ form.school || "—" }}</div>
              </div>
            </div>

            <!-- <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Grade Level</div>
                <div class="detail-item__value">{{ form.gradeLevel || "—" }}</div>
              </div>
            </div> -->
            <div class="col-12 col-sm-6">
  <div class="detail-item">
    <div class="detail-item__label">Grade Level</div>
    <div class="detail-item__value">{{ displayGradeLevel }}</div>
  </div>
</div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Transportation</div>
                <div class="detail-item__value">{{ form.transportation || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">T-Shirt Size</div>
                <div class="detail-item__value">{{ form.tShirtSize || "—" }}</div>
              </div>
            </div>
          </div>
        </section>

        <!-- Fee -->
        <section class="detail-section">
          <div class="detail-section__title">
            <q-icon name="o_payments" />
            <span>FEE</span>
          </div>

          <div class="row q-col-gutter-md">
            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Fee Amount</div>
                <div class="detail-item__value">{{ form.feeAmount ?? "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Fee Expiry Date</div>
                <div class="detail-item__value">{{ formatDate(form.feeExpiryDate) }}</div>
              </div>
            </div>

            <div class="col-12">
              <div class="detail-item">
                <div class="detail-item__label">Fee Note</div>
                <div class="detail-item__value">{{ form.feeNote || "—" }}</div>
              </div>
            </div>
          </div>
        </section>

        <!-- Medical -->
        <section class="detail-section">
          <div class="detail-section__title">
            <q-icon name="o_health_and_safety" />
            <span>MEDICAL</span>
          </div>

          <div class="row q-col-gutter-md">
            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Primary Doctor</div>
                <div class="detail-item__value">{{ form.primaryDoctor || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Health Insurance Carrier</div>
                <div class="detail-item__value">{{ form.healthInsuranceCarrier || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Has Immunizations?</div>
                <div class="detail-item__value">{{ form.hasImmunizations || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Immunizations</div>
                <div class="detail-item__value">{{ form.immunizationNotes || "—" }}</div>
              </div>
            </div>

            <div class="col-12">
              <div class="detail-item">
                <div class="detail-item__label">Medications</div>
                <div class="detail-item__value">{{ form.medications || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Disabilities</div>
                <div class="detail-item__value">{{ form.disabilitiesNotes || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Allergies</div>
                <div class="detail-item__value">{{ form.allergiesNotes || "—" }}</div>
              </div>
            </div>

            <div class="col-12">
              <div class="detail-item">
                <div class="detail-item__label">Special Needs</div>
                <div class="detail-item__value">{{ form.specialNeeds || "—" }}</div>
              </div>
            </div>
          </div>
        </section>

        <!-- Notes -->
        <section class="detail-section">
          <div class="detail-section__title">
            <q-icon name="o_notes" />
            <span>NOTES</span>
          </div>

          <div class="row q-col-gutter-md">
            <div class="col-12">
              <div class="detail-item">
                <div class="detail-item__label">Skill Notes</div>
                <div class="detail-item__value">{{ form.skillNotes || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Text Opt-In</div>
                <div class="detail-item__value">{{ form.textOptIn || "—" }}</div>
              </div>
            </div>

            <div class="col-12 col-sm-6">
              <div class="detail-item">
                <div class="detail-item__label">Mass Email Opt-Out</div>
                <div class="detail-item__value">{{ form.massEmailOptOut || "—" }}</div>
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { studentApi, studentGradeLevelApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useDateFormat } from "composables/useDateFormat";

import AppFormDialog from "components/common/AppFormDialog.vue";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  id: { type: [String, Number], default: null }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const { formatDate } = useDateFormat();

// Grade level options state
const gradeLevels = ref([]);
const loadGradeLevels = async () => {
  try {
    const res = await studentGradeLevelApi.list({ limit: 100 });
    gradeLevels.value = res?.data || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};


const loading = ref(false);

const dialogOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit("update:modelValue", value)
});

const blankForm = () => ({
  firstName: "",
  lastName: "",
  familyName: "",
  studentNumber: "",
  admissionDate: "",
  birthDate: "",
  gender: "",
  allowTextMessaging: true,
  email: "",
  cellPhone: "",
  school: "",
  gradeLevel: "",
  transportation: "",
  tShirtSize: "",
  feeAmount: null,
  feeExpiryDate: "",
  feeNote: "",
  primaryDoctor: "",
  healthInsuranceCarrier: "",
  hasImmunizations: "Yes",
  medications: "",
  immunizationNotes: "",
  disabilitiesNotes: "",
  allergiesNotes: "",
  specialNeeds: "",
  skillNotes: "",
  textOptIn: "",
  massEmailOptOut: "",
  active: true,
  location: "",
  weeklyHours: "",
  primaryContact: "",
  emergencyContact: "",
  classes: []
});

const form = reactive(blankForm());

const studentName = computed(() => {
  const name = `${form.firstName || ""} ${form.lastName || ""}`.trim();
  return name || "View Student";
});

const studentInitials = computed(() => {
  if (!studentName.value || studentName.value === "View Student") {
    return "ST";
  }

  const parts = studentName.value.split(/\s+/);

  if (parts.length === 1) {
    return parts[0].substring(0, 2).toUpperCase();
  }

  return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
});

const studentAge = computed(() => {
  if (!form.birthDate) {
    return "—";
  }

  const birthDate = new Date(form.birthDate);
  const today = new Date();

  let age = today.getFullYear() - birthDate.getFullYear();
  const monthDifference = today.getMonth() - birthDate.getMonth();

  if (
    monthDifference < 0 ||
    (monthDifference === 0 && today.getDate() < birthDate.getDate())
  ) {
    age--;
  }

  return age >= 0 ? age : "—";
});

const resetForm = () => {
  Object.assign(form, blankForm());
};

const loadStudent = async () => {
  if (!props.id) {
    resetForm();
    return;
  }

  loading.value = true;

  try {
   
   // const response = await studentApi.get(props.id);
   const [response] = await Promise.all([
      studentApi.get(props.id),
      loadGradeLevels()
    ]);

    Object.assign(form, {
      firstName: response?.firstName || "",
      lastName: response?.lastName || "",
      familyName: response?.familyName || "",
      studentNumber: response?.studentNumber || "",
      admissionDate: response?.admissionDate || "",
      birthDate: response?.birthDate || "",
      gender: response?.gender || "",
      allowTextMessaging: response?.allowTextMessaging ?? true,
      email: response?.email || "",
      cellPhone: response?.cellPhone || "",
      school: response?.school || "",
      gradeLevel: response?.gradeLevel || "",
      transportation: response?.transportation || "",
      tShirtSize: response?.tShirtSize || "",
      feeAmount: response?.feeAmount ?? null,
      feeExpiryDate: response?.feeExpiryDate || "",
      feeNote: response?.feeNote || "",
      primaryDoctor: response?.primaryDoctor || "",
      healthInsuranceCarrier: response?.healthInsuranceCarrier || "",
      hasImmunizations: response?.hasImmunizations || "Yes",
      medications: response?.medications || "",
      immunizationNotes: response?.immunizationNotes || "",
      disabilitiesNotes: response?.disabilitiesNotes || "",
      allergiesNotes: response?.allergiesNotes || "",
      specialNeeds: response?.specialNeeds || "",
      skillNotes: response?.skillNotes || "",
      textOptIn: response?.textOptIn || "",
      massEmailOptOut: response?.massEmailOptOut || "",
      active: response?.active ?? true,
      location: response?.location || response?.locationName || "",
      weeklyHours: response?.weeklyHours || "",
      primaryContact: response?.primaryContact || "",
      emergencyContact: response?.emergencyContact || "",
      classes: response?.classes || [],
      gradeLevel: response?.gradeLevel || response?.gradeLevelId || "",
    });
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    dialogOpen.value = false;
  } finally {
    loading.value = false;
  }
};

const studentSubtitle = computed(() => {
  const studentId = form.studentNumber ? `#${form.studentNumber}` : "#—";
  const status = form.active ? "Active" : "Inactive";

  return `Student ID: ${studentId} • ${status} • Enrolled Student`;
});

// Display grade level with fallback to raw value if not found in gradeLevels
const displayGradeLevel = computed(() => {
  const val = form.gradeLevel;
  if (!val) return "—";
  if (isNaN(val) && typeof val === "string" && !gradeLevels.value.some(g => (g.id || g.gradeLevelId) === val)) {
    return val;
  }
  const found = gradeLevels.value.find(
    (g) => (g.id || g.gradeLevelId) === val || String(g.id || g.gradeLevelId) === String(val)
  );

  return found ? (found.name || found.gradeName) : val;
});

// Watchers
watch(
  () => props.modelValue,
  (value) => {
    if (value) {
      loadStudent();
    } else {
      resetForm();
    }
  }
);

watch(
  () => props.id,
  () => {
    if (props.modelValue) {
      loadStudent();
    }
  }
);
</script>
