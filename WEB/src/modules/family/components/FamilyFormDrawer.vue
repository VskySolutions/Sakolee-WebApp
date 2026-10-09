<template>
  <!-- Create / Edit popup (read-only View lives in FamilyViewDrawer) -->
  <app-form-dialog
    v-model="isOpen"
    :title="editing ? 'Edit Family' : 'Create Family'"
    :subtitle="editing
      ? 'Update the family details, contacts, address, and students.'
      : 'Create a new family with its contacts, address, and students.'"
    :saving="saving"
    :save-label="editing ? 'Update Family' : 'Save Family'"
    size="lg"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-tabs v-model="activeTab" dense no-caps align="justify" class="text-grey-7" active-color="primary" indicator-color="primary">
      <q-tab name="family" label="Family Info" />
      <q-tab name="contacts" label="Contacts" />
      <q-tab name="address" label="Address & Emergency" />
      <q-tab name="students" label="Students" />
      <q-tab v-if="editing" name="enrollment" label="Class Enrollment" />
    </q-tabs>
    <q-separator class="q-mb-md" />

    <!-- All four tabs' fields stay mounted (toggled with v-show, not v-if) so the single q-form
         below can validate every field regardless of which tab is showing — an unmounted required
         field in an unvisited tab would silently pass validation otherwise. -->
    <q-form ref="formRef" greedy>
      <div v-show="activeTab === 'family'" class="row q-col-gutter-md">
        <app-text-field v-model="form.familyName" label="Family Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Family name is required', nameOnlyRule]" @keypress="(e) => { if (!/^[A-Za-z\s]$/.test(e.key)) e.preventDefault(); }"/>
        <app-select v-model="form.studioLocationId" label="Studio Location" :options="locationOptions" class="col-12 col-sm-6" />
        <!-- <app-select v-model="form.familyStatusId" label="Family Status" :options="familyStatusOptions" class="col-12 col-sm-6" /> -->
         <app-select v-model="form.familyStatusId" label="Family Status" :options="filteredFamilyStatusOptions" class="col-12 col-sm-6" />
        <app-select v-model="form.source" label="How Did You Hear About Us?" :options="sourceOptions" class="col-12 col-sm-6" />
        <app-text-field v-model="form.referralName" label="Referral Name" class="col-12 col-sm-6" :rules="[nameOnlyRule]" @keypress="(e) => { if (!/^[A-Za-z\s]$/.test(e.key)) e.preventDefault(); }"/>

        <div v-if="editing" class="col-12">
          <q-toggle v-model="form.active" label="Active" />
        </div>
      </div>

      <div v-show="activeTab === 'contacts'" class="row q-col-gutter-md">
        <div class="col-12 text-subtitle2 text-grey-8">Contact #1 (Primary)</div>
        <app-text-field v-model="form.firstName" label="First Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'First name is required',nameOnlyRule]" @keypress="(e) => { if (!/^[A-Za-z\s]$/.test(e.key)) e.preventDefault(); }"/>
        <app-text-field v-model="form.lastName" label="Last Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Last name is required',nameOnlyRule]" @keypress="(e) => { if (!/^[A-Za-z\s]$/.test(e.key)) e.preventDefault(); }"/>
        <app-select v-model="form.relation" label="Relation" :options="relationOptions" class="col-12 col-sm-6" />
        <app-text-field
          v-model="form.email" label="Email" type="email" required class="col-12 col-sm-6"
          :error="!!primaryEmailError" :error-message="primaryEmailError"
          :rules="[(v) => !!v || 'Email is required',emailRule]"
          hint="A login account is created for this contact."
        />
        <app-phone-input v-model="form.homePhone" label="Home Phone" class="col-12 col-sm-6 "  />
        <app-phone-input v-model="form.workPhone" label="Work Phone" class="col-12 col-sm-6" />
        <app-phone-input v-model="form.cellPhone" label="Cell Phone" class="col-12 col-sm-6" />
        <app-phone-input v-model="form.otherPhone" label="Other Phone" class="col-12 col-sm-6" />
        <app-text-field v-model="form.fax" label="Fax" class="col-12 col-sm-6" />
        <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.isBillingContact" color="primary" /><span class="q-ml-sm">Billing contact</span></div>
        <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.isAuthorizedToPickUpStudent" color="primary" /><span class="q-ml-sm">Authorized to pick up student</span></div>

        <div class="col-12 row items-center justify-between q-mt-sm">
          <div class="text-subtitle2 text-grey-8">Contact #2 (Secondary / Optional)</div>
          <q-btn v-if="!hasSecondaryContact" flat dense no-caps color="primary" icon="o_add" label="Add second contact" @click="addSecondaryContact" />
          <q-btn v-else-if="hasSecondaryContact && !editing" flat dense no-caps color="negative" icon="o_close" label="Remove" @click="removeSecondaryContact" />
        </div>
        <template v-if="hasSecondaryContact">
          <app-text-field v-model="form.secondaryContact.firstName" label="First Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'First name is required',nameOnlyRule]" @keypress="(e) => { if (!/^[A-Za-z\s]$/.test(e.key)) e.preventDefault(); }" />
          <app-text-field v-model="form.secondaryContact.lastName" label="Last Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Last name is required',nameOnlyRule]" @keypress="(e) => { if (!/^[A-Za-z\s]$/.test(e.key)) e.preventDefault(); }" />
          <app-select v-model="form.secondaryContact.relation" label="Relation" :options="relationOptions" class="col-12 col-sm-6" />
          <app-text-field
            v-model="form.secondaryContact.email" label="Email" type="email" required class="col-12 col-sm-6"
            :error="!!secondaryEmailError" :error-message="secondaryEmailError"
            :rules="[(v) => !!v || 'Email is required']"
            hint="A login account is created for this contact."
          />
          <app-text-field v-model="form.secondaryContact.phone" label="Cell Phone" class="col-12 col-sm-6" />
          <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.secondaryContact.isBillingContact" color="primary" /><span class="q-ml-sm">Billing contact</span></div>
          <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.secondaryContact.isAuthorizedToPickUpStudent" color="primary" /><span class="q-ml-sm">Authorized to pick up student</span></div>
        </template>
      </div>

      <div v-show="activeTab === 'address'" class="row q-col-gutter-md">
        <div class="col-12 text-subtitle2 text-grey-8">Household Address</div>
        <!-- Same Country → State → City field-set as the Tenant form; saved to the Addresses table. -->
        <div class="col-12">
          <app-address-fields v-model="form.address" />
        </div>

        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Emergency Contact</div>
        <app-text-field v-model="form.emergencyContactPerson" label="Emergency Contact Person" class="col-12 col-sm-6" :rules="[nameOnlyRule]" @keypress="(e) => { if (!/^[A-Za-z\s]$/.test(e.key)) e.preventDefault(); }"/>
        <app-phone-input v-model="form.emergencyPhone" label="Emergency Phone" class="col-12 col-sm-6" />
        <!-- <app-text-field v-model="form.healthInsuranceCarrier" label="Health Insurance Carrier / Policy #" class="col-12" />  &amp; Health Insurance-->
      </div>

      <div v-show="activeTab === 'students'" class="row q-col-gutter-md">
        <template v-if="students.length">
          <div class="col-12 text-subtitle2 text-grey-8">Enrolled Students</div>
          <div class="col-12">
            <q-list bordered separator>
              <q-item v-for="s in students" :key="s.studentId">
                <q-item-section>
                  <q-item-label>{{ s.firstName || s.lastName ? `${s.firstName || ''} ${s.lastName || ''}`.trim() : (s.studentNumber || 'Student') }}</q-item-label>
                  <q-item-label caption>{{ s.studentNumber || '—' }}</q-item-label>
                </q-item-section>
                <q-item-section side class="row items-center q-gutter-xs">
                  <q-badge :color="s.active ? 'positive' : 'grey'">{{ s.active ? 'Active' : 'Inactive' }}</q-badge>
                  <q-btn flat round dense color="primary" icon="o_edit" @click="openStudentEdit(s.studentId)">
                    <q-tooltip>Edit student</q-tooltip>
                  </q-btn>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
        </template>

        <!-- Add one or more new students under this family — mirrors Quick Registration's Step 4,
             reusing the same blankStudent()/studentApi.createBulk() so both entry points behave the
             same way. Only offered when editing an existing family. -->
        <template v-if="editing">
          <div class="col-12 row items-center justify-between q-mt-sm">
            <div class="text-subtitle2 text-grey-8">Add Student(s)</div>
            <q-btn flat dense no-caps color="primary" icon="o_add" label="Add Student" @click="addNewStudent" />
          </div>
          <div v-for="(student, index) in form.newStudents" :key="student.key" class="col-12">
            <div class="row items-center justify-between q-mb-xs">
              <div class="text-caption text-weight-bold text-primary">
                Student #{{ index + 1 }}{{ studentDisplayName(student) ? ` — ${studentDisplayName(student)}` : "" }}
              </div>
              <q-btn flat dense no-caps color="negative" icon="o_close" label="Remove" @click="removeNewStudent(index)" />
            </div>
            <div class="row q-col-gutter-md q-mb-sm">
              <app-text-field v-model="student.firstName" label="First Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'First name is required', nameOnlyRule]" @keypress="(e) => { if (!/^[A-Za-z\s]$/.test(e.key)) e.preventDefault(); }" />
              <app-text-field v-model="student.lastName" label="Last Name" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Last name is required', nameOnlyRule]" @keypress="(e) => { if (!/^[A-Za-z\s]$/.test(e.key)) e.preventDefault(); }" />
              <app-text-field
                v-model="student.email" label="Student Email" type="email" required class="col-12 col-sm-6"
                :rules="[(v) => !!v || 'Email is required',]"
                hint="A separate login account is created for the student with this email."
              />
              <app-date-field v-model="student.birthDate" label="Birth Date" required class="col-12 col-sm-6" :rules="[(v) => !!v || 'Birth date is required']" />
              <app-select v-model="student.gender" label="Gender" :options="genderOptions" class="col-12 col-sm-6" />
              <app-select v-model="student.tshirtSize" label="T-Shirt Size" :options="tshirtSizeOptions" class="col-12 col-sm-6" />
              <!-- <app-text-field v-model="student.gradeLevel" label="Grade Level" class="col-12 col-sm-6" /> -->
               <app-select v-model="student.gradeLevel" label="Grade Level" :options="gradeLevelOptions" class="col-12 col-sm-6" />
              <app-text-field v-model="student.medicalNotes" label="Allergies, Special Needs &amp; Medical Notes" type="textarea" class="col-12" />
            </div>
            <q-separator v-if="index < form.newStudents.length - 1" />
          </div>
        </template>

        <div v-if="!students.length && !form.newStudents.length" class="col-12 text-grey-6 text-caption">
          No students enrolled yet.
        </div>
      </div>

      <!-- Class enrollment for this family's students — the Edit Family counterpart of Quick
           Registration's Step 5. An existing student's class is changed through Edit Student; a student
           added on the Students tab is enrolled here and sent with the same createBulk() call. -->
      <div v-if="editing" v-show="activeTab === 'enrollment'" class="row q-col-gutter-md">
        <template v-if="students.length">
          <div class="col-12 text-subtitle2 text-grey-8">Current Enrollments</div>
          <div class="col-12">
            <q-list bordered separator>
              <q-item v-for="s in students" :key="s.studentId">
                <q-item-section>
                  <q-item-label>{{ s.firstName || s.lastName ? `${s.firstName || ''} ${s.lastName || ''}`.trim() : (s.studentNumber || 'Student') }}</q-item-label>
                  <q-item-label caption>{{ studentClassesLabel(s) }}</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-btn flat round dense color="primary" icon="o_edit" @click="openStudentEdit(s.studentId, true)">
                    <q-tooltip>Change classes</q-tooltip>
                  </q-btn>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
        </template>

        <!-- <template v-if="validNewStudents.length">
          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">New Student Enrollment</div>
          <div v-for="(student, index) in validNewStudents" :key="student.key" class="col-12">
            <div class="text-caption text-weight-bold text-primary q-mb-xs">
              Student #{{ index + 1 }}{{ studentDisplayName(student) ? ` — ${studentDisplayName(student)}` : "" }}
            </div>
            <div class="row q-col-gutter-md q-mb-sm">
              <app-select v-model="student.classIds" label="Choose Classes" :options="getFilteredClassesForStudent(student)" multiple class="col-12 col-sm-6" />
              <app-date-field v-model="student.enrollmentDate" label="Enrollment Date" class="col-12 col-sm-6" />
            </div>
          </div>
        </template> -->

        <template v-if="validNewStudents.length">
          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">New Student Enrollment</div>
          <div v-for="(student, index) in validNewStudents" :key="student.key" class="col-12">
            <div class="text-caption text-weight-bold text-primary q-mb-xs">
              Student #{{ index + 1 }}{{ studentDisplayName(student) ? ` — ${studentDisplayName(student)}` : "" }}
            </div>
            <div class="row q-col-gutter-md q-mb-sm">
              <app-select 
                v-model="student.classIds" 
                label="Choose Classes" 
                :options="getFilteredClassesForStudent(student)" 
                multiple 
                class="col-12 col-sm-6" 
              />
              <app-date-field v-model="student.enrollmentDate" label="Enrollment Date" class="col-12 col-sm-6" />
            </div>
          </div>
        </template>

        <div v-if="!students.length && !validNewStudents.length" class="col-12 text-grey-6 text-caption">
          No students to enroll yet. Add a student on the Students tab first.
        
      </div>

        <!-- <template v-if="form.newStudents.length">
          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">New Student Enrollment</div>
          <div v-for="(student, index) in form.newStudents" :key="student.key" class="col-12">
            <div class="text-caption text-weight-bold text-primary q-mb-xs">
              Student #{{ index + 1 }}{{ studentDisplayName(student) ? ` — ${studentDisplayName(student)}` : "" }}
            </div>
            <div class="row q-col-gutter-md q-mb-sm">
              <app-select v-model="student.classIds" label="Choose Classes" :options="classOptions" multiple class="col-12 col-sm-6" />
              <app-date-field v-model="student.enrollmentDate" label="Enrollment Date" class="col-12 col-sm-6" />
            </div>
          </div>
        </template> -->

        <div v-if="!students.length && !form.newStudents.length" class="col-12 text-grey-6 text-caption">
          No students to enroll yet. Add a student on the Students tab first.
        </div>
      </div>
    </q-form>
  </app-form-dialog>

  <student-edit-dialog v-model="studentEditOpen" :student-id="studentEditId" :enrollment-only="studentEditEnrollmentOnly" @saved="onStudentSaved" />

  <!-- Temporary passwords for newly-created contact logins -->
  <q-dialog v-model="tempPwOpen" persistent>
    <q-card style="min-width: 420px;">
      <q-card-section class="row items-center q-gutter-sm">
        <q-icon name="o_key" color="primary" size="sm" />
        <div class="text-h6">Temporary passwords</div>
      </q-card-section>
      <q-card-section>
        <div class="text-body2 text-grey-7 q-mb-sm">These will not be shown again. Share them with each contact securely.</div>
        <div v-for="c in tempPasswords" :key="c.email" class="q-mb-sm">
          <div class="text-caption text-grey-7">{{ c.email }}</div>
          <q-input :model-value="c.password" readonly outlined dense>
            <template #append>
              <q-btn flat round dense icon="o_content_copy" @click="copyPassword(c.password)">
                <q-tooltip>Copy</q-tooltip>
              </q-btn>
            </template>
          </q-input>
        </div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn v-close-popup flat no-caps color="primary" label="Done" @click="onTempPasswordsClosed" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { familyApi, locationApi, classApi, studentApi, studentGradeLevelApi, familyRelationApi,getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { blankFamilyForm, blankSecondaryContact, familyFormFromDetail } from "composables/familyForm";
import { RELATION_OPTIONS, GENDER_OPTIONS, blankStudent } from "composables/quickRegistrationForm";
import { useTShirtSizeOptions } from "composables/useTShirtSizeOptions";
import { useHearAboutUsOptions } from "composables/useHearAboutUsOptions";
import AppPhoneInput from "components/common/AppPhoneInput.vue";

import AppFormDialog from "components/common/AppFormDialog.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppDateField from "components/common/AppDateField.vue";
import AppAddressFields from "components/common/AppAddressFields.vue";
import { hasAddressValue } from "utils/address";
import StudentEditDialog from "modules/family/components/StudentEditDialog.vue";

const props = defineProps({
  modelValue: { type: Boolean, required: true },
  // "create" | "edit"
  mode: { type: String, default: "create" },
  familyId: { type: [String, Number], default: null },
  // Owned by the list page (it also drives the Family Status filter there).
  familyStatusOptions: { type: Array, default: () => [] }
});

const filteredFamilyStatusOptions = computed(() => {
  return (props.familyStatusOptions || []).filter((s) => s.active ?? s.Active ?? true);
});

const NAME_RE = /^[A-Za-z\s]+$/;

const nameOnlyRule = (val) => {
  if (!val) return true;
  const str = String(val);
  if (!NAME_RE.test(str)) {
    return "Only letters are allowed";
  }
  if (str.length > 30) {
    return "Maximum 30 characters allowed";
  }
  return true;
};

//Rules for validating email addresses. The optionalEmail rule allows empty values, but if a value is present, it must be a valid email.
const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const emailRule = (val) => EMAIL_RE.test(String(val || "")) || "Enter a valid email";

// Grade level options state
const gradeLevels = ref([]);
const gradeLevelOptions = computed(() => {
  const options = (gradeLevels.value || []).filter((g) => g.active ?? g.Active ?? true).map((g) => ({ label: g.Name || g.gradeName, value: g.Id || g.gradeLevelId }));
  return options;
});

// Family relation options state
const familyRelations = ref([]);
const relationOptions = computed(() => {
  return (familyRelations.value || [])
    .filter((r) => r.active ?? r.Active ?? true)
    .map((r) => ({ 
      label: r.name || r.relationName || r.Name, 
      value: r.id || r.familyRelationId || r.Id 
    }));
});


const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();

const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});
const editing = computed(() => props.mode === "edit");

// Only new students with at least one field filled in are considered for enrollment; the rest are ignored.
const validNewStudents = computed(() => {
  return (form.newStudents || []).filter(s => s.firstName || s.lastName || s.email || s.birthDate);
});

const studentDisplayIndex = (student) => {
  const originalIndex = form.newStudents.findIndex(s => s.key === student.key);
  return originalIndex !== -1 ? originalIndex + 1 : 1;
};


// Calculates age from birth date string, returns null if invalid or not provided
const calculateAge = (birthDateString) => {
  if (!birthDateString) return null;
  const birthDate = new Date(birthDateString);
  const today = new Date();
  if (isNaN(birthDate.getTime())) return null;

  let age = today.getFullYear() - birthDate.getFullYear();
  const m = today.getMonth() - birthDate.getMonth();
  if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
    age--;
  }
  return age;
};

// Filters available classes based on the student's age range safely
const getFilteredClassesForStudent = (studentOrForm) => {
  if (!studentOrForm) return [];

  const studentAge = calculateAge(studentOrForm.birthDate);

  const filtered = (classes.value || []).filter((c) => {
    // Check if the class is active
    const isActive = c.active ?? c.Active ?? c.IsActive ?? true;
    if (!isActive) return false;

    // If student's age is not provided, show all active classes
    if (studentAge === null) return true;

    // Get min and max age (handle null/undefined fields safely from DB)
    const rawMin = c.minAge ?? c.minimumAge ?? c.MinAge ?? c.min_age;
    const rawMax = c.maxAge ?? c.maximumAge ?? c.MaxAge ?? c.max_age;

    // If min/max are not set (NULL in DB), class is open for all ages
    const min = (rawMin !== null && rawMin !== undefined && rawMin !== "") ? Number(rawMin) : 0;
    const max = (rawMax !== null && rawMax !== undefined && rawMax !== "") ? Number(rawMax) : 999;

    // Check if student's age falls within this class range
    return studentAge >= min && studentAge <= max;
  });

  // Map to select options format
  return filtered.map((c) => ({
    label: c.className || c.name || c.Name || c.class_name || "Unnamed Class",
    value: c.classId || c.id || c.Id || c.class_id
  }));
};

// ---- Reference option lists ----
const locations = ref([]);
// Same list as the Class form's Location. A family whose saved location has since been deactivated
// still shows it by name (from the family record) rather than as a raw id.
const locationOptions = computed(() => {
  const options = (locations.value || []).filter((l) => l.active ?? l.Active ?? true).map((l) => ({ label: l.name, value: l.id }));
  if (form.studioLocationId && form.studioLocationName && !options.some((o) => o.value === form.studioLocationId)) {
    options.unshift({ label: form.studioLocationName, value: form.studioLocationId });
  }
  return options;
});
// Real classes for enrolling a newly-added student, loaded the same way Quick Registration's Step 5 does.
const classes = ref([]);
const classOptions = computed(() => classes.value.map((c) => ({ label: c.className, value: c.classId })));
const className = (classId) => classes.value.find((c) => c.classId === classId)?.className || "";
// Every class a student is in (a student may be in several); falls back to the legacy single classId.
const studentClassesLabel = (s) => {
  const ids = s.classIds?.length ? s.classIds : s.classId ? [s.classId] : [];
  if (!ids.length) return "Not enrolled in a class";
  return ids.map((id) => className(id) || "Enrolled (inactive class)").join(", ");
};
//const relationOptions = RELATION_OPTIONS;
//const { options: sourceOptions } = useHearAboutUsOptions(() => form.source);
const { options: sourceOptions } = useHearAboutUsOptions();
const genderOptions = GENDER_OPTIONS;
const { options: tshirtSizeOptions } = useTShirtSizeOptions();

onMounted(async () => {
  try {
    const [locRes, classRes, gradeRes,familyRelationRes] = await Promise.all([
      locationApi.list({ limit: 100, active: true }),
      classApi.list({ limit: 100, active: true }),
      studentGradeLevelApi.list({ limit: 100, active: true }),
      familyRelationApi.list({ limit: 100, active: true })
    ]);
    locations.value = locRes?.data || [];
    classes.value = classRes?.data || [];
    gradeLevels.value = gradeRes?.data || [];
    familyRelations.value = familyRelationRes?.data || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
});

// ---- Create / Edit / View ----
const saving = ref(false);
const primaryEmailError = ref("");
const secondaryEmailError = ref("");
const formRef = ref(null);
const students = ref([]);
const activeTab = ref("family");

// ---- Enrolled-student edit (the only place a student's details can be edited now — see
// StudentEditDialog's remarks) ----
const studentEditOpen = ref(false);
const studentEditId = ref(null);
// Class Enrollment tab opens the dialog with only the class selection; the Students tab, the full form.
const studentEditEnrollmentOnly = ref(false);
const openStudentEdit = (studentId, enrollmentOnly = false) => {
  studentEditId.value = studentId;
  studentEditEnrollmentOnly.value = enrollmentOnly;
  studentEditOpen.value = true;
};
const onStudentSaved = async () => {
  // Refresh this family's own student list so the edited name/status shows immediately.
  if (!props.familyId) return;
  try {
    const detail = await familyApi.get(props.familyId);
    students.value = detail.students || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const form = reactive(blankFamilyForm());
const hasSecondaryContact = computed(() => !!form.secondaryContact);
const addSecondaryContact = () => { form.secondaryContact = blankSecondaryContact(); };
const removeSecondaryContact = () => { form.secondaryContact = null; };
const addNewStudent = () => { form.newStudents.push({ ...blankStudent(), enrollmentDate: "" }); };
const removeNewStudent = (index) => { form.newStudents.splice(index, 1); };
const studentDisplayName = (student) => `${student.firstName || ""} ${student.lastName || ""}`.trim();

const resetForm = () => {
  Object.assign(form, blankFamilyForm());
  primaryEmailError.value = "";
  secondaryEmailError.value = "";
  students.value = [];
  activeTab.value = "family";
  formRef.value?.resetValidation?.();
};

const populateFrom = (detail) => {
  Object.assign(form, familyFormFromDetail(detail));
  //form.familyStatusId = detail.familyStatusId || detail.FamilyStatusId || null;
  students.value = detail.students || [];
};

// Each time the popup opens, start from a clean form and (for Edit) load the family.
watch(
  () => props.modelValue,
  async (val) => {
    if (!val) return;
    resetForm();
    if (props.mode === "create" || !props.familyId) return;
    try {
      const detail = await familyApi.get(props.familyId);
      populateFrom(detail);
    } catch (err) {
      notify.error(getApiErrorMessage(err));
    }
  }
);

const toSecondaryContactPayload = (contact) => ({
  firstName: contact.firstName,
  lastName: contact.lastName,
  email: contact.email,
  phone: contact.phone || null,
  relation: contact.relation || null,
  isBillingContact: contact.isBillingContact,
  isAuthorizedToPickUpStudent: contact.isAuthorizedToPickUpStudent
});

const closeAndNotifySaved = () => {
  isOpen.value = false;
  resetForm();
  emit("saved");
};

const tempPwOpen = ref(false);
const tempPasswords = ref([]);
const copyPassword = async (password) => {
  try {
    await navigator.clipboard.writeText(password);
    notify.success("Copied to clipboard.");
  } catch {
    notify.warning("Copy failed — please select and copy manually.");
  }
};
const onTempPasswordsClosed = () => closeAndNotifySaved();

const submitForm = async () => {
  primaryEmailError.value = "";
  secondaryEmailError.value = "";
  const valid = await formRef.value?.validate();
  if (!valid) {
    // All four tabs' fields stay mounted (v-show), so validate() above already checked every one —
    // this only decides which tab to land the user on so they can actually see what failed.
    if (!form.familyName) {
      activeTab.value = "family";
    } else if (
      !form.firstName || !form.lastName || !form.email ||
      (form.secondaryContact && (!form.secondaryContact.firstName || !form.secondaryContact.lastName || !form.secondaryContact.email))
    ) {
      activeTab.value = "contacts";
    } else if (form.newStudents.some((s) => !s.firstName || !s.lastName || !s.email || !s.birthDate)) {
      activeTab.value = "students";
    }
    return;
  }

  // Every contact and every new student needs its own distinct email — checked up front, before any
  // API call, the same way Quick Registration's own submit does.
  const allEmails = [
    form.email,
    ...(form.secondaryContact?.email ? [form.secondaryContact.email] : []),
    ...form.newStudents.map((s) => s.email)
  ].map((e) => e.trim().toLowerCase());
  const firstDuplicate = allEmails.find((email, i) => allEmails.indexOf(email) !== i);
  if (firstDuplicate) {
    notify.warning(`"${firstDuplicate}" is used more than once — every contact and student needs its own distinct email.`);
    return;
  }

  const payload = {
    familyName: form.familyName,
    studioLocationId: form.studioLocationId || null,
    familyStatusId: form.familyStatusId || null,
    source: form.source || null,
    referralName: form.referralName || null,
    firstName: form.firstName,
    lastName: form.lastName,
    email: form.email,
    relation: form.relation || null,
    homePhone: form.homePhone || null,
    workPhone: form.workPhone || null,
    cellPhone: form.cellPhone || null,
    otherPhone: form.otherPhone || null,
    fax: form.fax || null,
    isBillingContact: form.isBillingContact,
    isAuthorizedToPickUpStudent: form.isAuthorizedToPickUpStudent,
    // An untouched address block is left out (on update that keeps the saved address unchanged).
    address: hasAddressValue(form.address) ? form.address : undefined,
    emergencyContactPerson: form.emergencyContactPerson || null,
    emergencyPhone: form.emergencyPhone || null,
    healthInsuranceCarrier: form.healthInsuranceCarrier || null,
    secondaryContact: form.secondaryContact ? toSecondaryContactPayload(form.secondaryContact) : null
  };

  saving.value = true;
  let result;
  try {
    if (editing.value) {
      result = await familyApi.update(props.familyId, { ...payload, active: form.active });
      notify.success("Family updated.");
    } else {
      result = await familyApi.create(payload);
      notify.success("Family created.");
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    saving.value = false;
    return;
  }

  const passwords = (result?.contacts || [])
    .filter((c) => c.temporaryPassword)
    .map((c) => ({ email: c.email, password: c.temporaryPassword }));

  // Any new students are created in one bulk call (StudentsController.CreateBulk) — the family (+ its
  // contacts) already saved above, so this is caught on its own: a failure here must not read as
  // "nothing happened."
  if (form.newStudents.length) {
    try {
      const studentPayload = form.newStudents.map((student) => ({
        familyId: result.familyId,
        familyName: form.familyName || null,
        firstName: student.firstName,
        lastName: student.lastName,
        email: student.email,
        birthDate: student.birthDate || null,
        gender: student.gender || null,
        tShirtSize: student.tshirtSize || null,
        gradeLevel: student.gradeLevel || null,
        specialNeeds: student.medicalNotes || null,
        classIds: student.classIds || [],
        admissionDate: student.enrollmentDate || null,
        healthInsuranceCarrier: form.healthInsuranceCarrier || null,
        emergencyContactName: form.emergencyContactPerson || null,
        emergencyContactNumber: form.emergencyPhone || null
      }));
      const studentResult = await studentApi.createBulk(studentPayload);
      (studentResult?.students || []).forEach((created, i) => {
        if (created?.temporaryPassword) {
          passwords.push({ email: form.newStudents[i].email, password: created.temporaryPassword });
        }
      });
      notify.success(`${form.newStudents.length} student(s) enrolled.`);
    } catch (err) {
      notify.error(`Family "${form.familyName}" was saved, but the new student(s) could not be enrolled: ${getApiErrorMessage(err)}`);
    }
  }

  saving.value = false;
  if (passwords.length) {
    tempPasswords.value = passwords;
    tempPwOpen.value = true;
    return; // close/reset/"saved" happen once the temp-password dialog is dismissed
  }

  closeAndNotifySaved();
};
</script>

<style scoped>
.toggle-row-inline {
  display: flex;
  align-items: center;
  min-height: 40px;
}
</style>
