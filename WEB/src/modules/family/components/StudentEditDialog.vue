<template>
  <q-dialog v-model="open" position="right">
    <q-card class="column no-wrap" style="width: 640px; max-width: 100vw; height: 100vh;">
      <q-card-section class="row items-center q-pb-none bg-primary text-white">
        <div class="text-h6">Edit Student</div>
        <q-space />
        <q-btn v-close-popup icon="o_close" flat round dense />
      </q-card-section>

      <q-card-section class="col q-pa-md scroll">
        <q-inner-loading :showing="loading" />
        <q-form v-if="!loading" ref="formRef" greedy>
          <div class="row q-col-gutter-md">
            <div class="col-12 text-subtitle2 text-grey-8">Identity</div>
            <app-text-field v-model="form.firstName" label="First Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'First name is required']" />
            <app-text-field v-model="form.lastName" label="Last Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Last name is required']" />
            <app-text-field v-model="form.familyName" label="Family Name" class="col-12 col-sm-6" />
            <app-text-field v-model="form.studentNumber" label="Student Number" class="col-12 col-sm-6" />
            <app-select v-model="form.gender" label="Gender" :options="GENDER_OPTIONS" class="col-12 col-sm-6" />
            <app-date-field v-model="form.birthDate" label="Date of Birth" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Date of birth is required']" />
            <app-date-field v-model="form.admissionDate" label="Admission Date" class="col-12 col-sm-6" />
            <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.allowTextMessaging" color="primary" /><span class="q-ml-sm">Allow text messaging</span></div>
            <app-text-field
              v-model="form.email" label="Email" required class="col-12 col-sm-6"
              :error="!!emailError" :error-message="emailError"
              :rules="[(v) => !!v || 'Email is required']"
              hint="This student's own login email."
            />
            <app-text-field v-model="form.cellPhone" label="Cell Phone" class="col-12 col-sm-6" />

            <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">School</div>
            <app-text-field v-model="form.school" label="School" class="col-12 col-sm-6" />
            <app-text-field v-model="form.gradeLevel" label="Grade Level" class="col-12 col-sm-6" />
            <app-text-field v-model="form.transportation" label="Transportation" placeholder="e.g. School Bus" class="col-12 col-sm-6" />
            <app-select v-model="form.tShirtSize" label="T-Shirt Size" :options="TSHIRT_SIZE_OPTIONS" class="col-12 col-sm-6" />

            <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Fee</div>
            <app-text-field v-model.number="form.feeAmount" label="Fee Amount" type="number" class="col-12 col-sm-6" />
            <app-date-field v-model="form.feeExpiryDate" label="Fee Expiry Date" class="col-12 col-sm-6" />
            <app-text-field v-model="form.feeNote" label="Fee Note" class="col-12" />

            <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Medical</div>
            <app-text-field v-model="form.primaryDoctor" label="Primary Doctor" class="col-12 col-sm-6" />
            <app-text-field v-model="form.healthInsuranceCarrier" label="Health Insurance Carrier" class="col-12 col-sm-6" />
            <app-select v-model="form.hasImmunizations" label="Has Immunizations?" :options="['Yes', 'No', 'Exempt']" class="col-12 col-sm-6" />
            <app-text-field v-model="form.immunizationNotes" label="Immunizations" class="col-12 col-sm-6" />
            <app-text-field v-model="form.medications" label="Medications" type="textarea" class="col-12" />
            <app-text-field v-model="form.disabilitiesNotes" label="Disabilities" class="col-12 col-sm-6" />
            <app-text-field v-model="form.allergiesNotes" label="Allergies" class="col-12 col-sm-6" />
            <app-text-field v-model="form.specialNeeds" label="Special Needs" type="textarea" class="col-12" />

            <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Notes</div>
            <app-text-field v-model="form.skillNotes" label="Skill Notes" type="textarea" class="col-12" />
            <app-text-field v-model="form.textOptIn" label="Text Opt-In" class="col-12 col-sm-6" />
            <app-text-field v-model="form.massEmailOptOut" label="Mass Email Opt-Out" class="col-12 col-sm-6" />

            <div class="col-12">
              <q-toggle v-model="form.active" label="Active" />
            </div>
          </div>
        </q-form>
      </q-card-section>

      <q-card-actions align="right" class="q-pa-md bg-grey-2">
        <q-btn v-close-popup flat no-caps label="Cancel" color="grey" />
        <q-btn unelevated no-caps color="primary" label="Save" :loading="saving" :disable="loading" @click="save" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
// A student's own full detail, editable — the only place a student's details can be edited now that
// the Students page itself is read-only (see StudentsController.Update and the Family page's
// "Enrolled Students" list, which opens this per row).
import { computed, reactive, ref, watch } from "vue";
import { studentApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";
import { GENDER_OPTIONS, TSHIRT_SIZE_OPTIONS } from "composables/quickRegistrationForm";

import AppTextField from "components/common/AppTextField.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppDateField from "components/common/AppDateField.vue";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  studentId: { type: String, default: null }
});
const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const open = computed({
  get: () => props.modelValue,
  set: (v) => emit("update:modelValue", v)
});

const loading = ref(false);
const saving = ref(false);
const emailError = ref("");
const formRef = ref(null);

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
  active: true
});
const form = reactive(blankForm());

// Fields this dialog does not edit. StudentsController.Update writes every field it is sent, so a
// missing one is saved as null — a missing familyId drops the student out of their family. They are
// carried over from the loaded record and sent back unchanged.
const blankPreserved = () => ({
  familyId: null,
  classId: null,
  feeCategoryId: null,
  disabilities: null,
  allergies: null,
  emergencyContactName: null,
  emergencyContactNumber: null
});
const preserved = reactive(blankPreserved());

// Loads the full student record fresh each time the dialog opens — the Family page's own student
// rows carry only the minimal FamilyStudentSummary shape (name, number, active, classId), not enough
// to populate a full edit form.
watch(() => props.modelValue, async (isOpen) => {
  if (!isOpen || !props.studentId) return;
  Object.assign(form, blankForm());
  Object.assign(preserved, blankPreserved());
  emailError.value = "";
  loading.value = true;
  try {
    const row = await studentApi.get(props.studentId);
    Object.assign(preserved, {
      familyId: row.familyId ?? null,
      classId: row.classId ?? null,
      feeCategoryId: row.feeCategoryId ?? null,
      disabilities: row.disabilities ?? null,
      allergies: row.allergies ?? null,
      emergencyContactName: row.emergencyContactName ?? null,
      emergencyContactNumber: row.emergencyContactNumber ?? null
    });
    Object.assign(form, {
      firstName: row.firstName || "",
      lastName: row.lastName || "",
      familyName: row.familyName || "",
      studentNumber: row.studentNumber || "",
      admissionDate: row.admissionDate ? row.admissionDate.substring(0, 10) : "",
      birthDate: row.birthDate ? row.birthDate.substring(0, 10) : "",
      gender: row.gender || "",
      allowTextMessaging: row.allowTextMessaging ?? true,
      email: row.email || "",
      cellPhone: row.cellPhone || "",
      school: row.school || "",
      gradeLevel: row.gradeLevel || "",
      transportation: row.transportation || "",
      tShirtSize: row.tShirtSize || "",
      feeAmount: row.feeAmount ?? null,
      feeExpiryDate: row.feeExpiryDate ? row.feeExpiryDate.substring(0, 10) : "",
      feeNote: row.feeNote || "",
      primaryDoctor: row.primaryDoctor || "",
      healthInsuranceCarrier: row.healthInsuranceCarrier || "",
      hasImmunizations: row.hasImmunizations || "Yes",
      medications: row.medications || "",
      immunizationNotes: row.immunizationNotes || "",
      disabilitiesNotes: row.disabilitiesNotes || "",
      allergiesNotes: row.allergiesNotes || "",
      specialNeeds: row.specialNeeds || "",
      skillNotes: row.skillNotes || "",
      textOptIn: row.textOptIn || "",
      massEmailOptOut: row.massEmailOptOut || "",
      active: row.active
    });
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    open.value = false;
  } finally {
    loading.value = false;
  }
});

const save = async () => {
  emailError.value = "";
  const valid = await formRef.value?.validate();
  if (!valid) return;

  const payload = {
    ...preserved,
    firstName: form.firstName,
    lastName: form.lastName,
    familyName: form.familyName || null,
    studentNumber: form.studentNumber || null,
    admissionDate: form.admissionDate || null,
    birthDate: form.birthDate || null,
    gender: form.gender || null,
    allowTextMessaging: form.allowTextMessaging,
    email: form.email || null,
    cellPhone: form.cellPhone || null,
    school: form.school || null,
    gradeLevel: form.gradeLevel || null,
    transportation: form.transportation || null,
    tShirtSize: form.tShirtSize || null,
    feeAmount: form.feeAmount,
    feeExpiryDate: form.feeExpiryDate || null,
    feeNote: form.feeNote || null,
    primaryDoctor: form.primaryDoctor || null,
    healthInsuranceCarrier: form.healthInsuranceCarrier || null,
    hasImmunizations: form.hasImmunizations || null,
    medications: form.medications || null,
    immunizationNotes: form.immunizationNotes || null,
    disabilitiesNotes: form.disabilitiesNotes || null,
    allergiesNotes: form.allergiesNotes || null,
    specialNeeds: form.specialNeeds || null,
    skillNotes: form.skillNotes || null,
    textOptIn: form.textOptIn || null,
    massEmailOptOut: form.massEmailOptOut || null,
    active: form.active
  };

  saving.value = true;
  try {
    await studentApi.update(props.studentId, payload);
    notify.success("Student updated.");
    open.value = false;
    emit("saved");
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      emailError.value = "A student with this email already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>

<style scoped>
.toggle-row-inline {
  display: flex;
  align-items: center;
  min-height: 40px;
}
</style>
