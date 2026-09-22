<template>
  <div class="app-field">
    <app-field-label :label="label" :required="required" />
    <q-input
      v-model="display"
      mask="##:## AA"
      placeholder="hh:mm AM/PM"
      :rules="rules"
      :error="error"
      :error-message="errorMessage"
      :disable="disable"
      :readonly="readonly"
      :hint="hint"
      :autocomplete="autocomplete"
      :aria-label="ariaLabel"
      outlined
      :dense="dense"
      hide-bottom-space
      class="app-time"
      @blur="onBlur"
    >
      <template #append>
        <!-- The whole point of the icon is that somebody can see it. -->
        <q-icon
          name="o_schedule"
          size="20px"
          :class="['app-time__icon', { 'app-time__icon--locked': locked }]"
        >
          <q-tooltip v-if="!locked">Pick a time</q-tooltip>
          <q-popup-proxy
            v-if="!locked"
            ref="popupRef"
            cover
            transition-show="jump-down"
            transition-hide="jump-up"
          >
            <q-time
              v-model="time24Model"
              mask="HH:mm"
              :format24h="false"
              now-btn
              color="primary"
              class="app-time__clock"
            >
              <div class="row items-center justify-end q-gutter-sm">
                <q-btn v-if="time24Model" flat dense no-caps size="sm" color="grey-8" label="Clear" @click="clear" />
                <q-btn v-close-popup flat dense no-caps size="sm" color="primary" label="Done" />
              </div>
            </q-time>
          </q-popup-proxy>
        </q-icon>
      </template>
    </q-input>
  </div>
</template>

<script setup>
// Standard time field. v-model is a "hh:mm AM/PM" string — what the Class record stores as free text
// (see StartTime/EndTime) — with the box enforcing that same 12-hour format on typed input and a clock
// popup as the alternative way in.
import { ref, computed, toRef } from "vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";
import { useFieldLabel } from "composables/useFieldLabel";

const props = defineProps({
  /** "hh:mm AM/PM", e.g. "04:42 AM". Empty string when unset. */
  modelValue: { type: String, default: "" },
  label: { type: String, default: "" },
  required: { type: Boolean, default: false },
  rules: { type: Array, default: () => [] },
  error: { type: Boolean, default: false },
  errorMessage: { type: String, default: "" },
  disable: { type: Boolean, default: false },
  readonly: { type: Boolean, default: false },
  hint: { type: String, default: "" },
  dense: { type: Boolean, default: true },
  autocomplete: { type: String, default: "off" }
});

const emit = defineEmits(["update:modelValue"]);

const popupRef = ref(null);

// Neither readonly nor disabled opens the clock: a field being read is being read, not filled in.
const locked = computed(() => props.readonly || props.disable);

const DISPLAY_RE = /^(\d{2}):(\d{2}) (AM|PM)$/;

// ---- "hh:mm AM/PM" ⇄ "HH:mm" (what q-time's model always uses, regardless of format24h) ----
const displayToTime24 = (text) => {
  const m = DISPLAY_RE.exec(String(text || "").trim());
  if (!m) return null;
  let hour = Number(m[1]);
  const minute = Number(m[2]);
  if (hour < 1 || hour > 12 || minute > 59) return null;
  if (m[3] === "AM") hour = hour === 12 ? 0 : hour;
  else hour = hour === 12 ? 12 : hour + 12;
  return `${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")}`;
};

const time24ToDisplay = (time24) => {
  const m = /^(\d{2}):(\d{2})$/.exec(String(time24 || "").trim());
  if (!m) return "";
  const hour = Number(m[1]);
  const minute = Number(m[2]);
  const period = hour >= 12 ? "PM" : "AM";
  const hour12 = hour % 12 || 12;
  return `${String(hour12).padStart(2, "0")}:${String(minute).padStart(2, "0")} ${period}`;
};

// What the box shows; kept in sync with modelValue whenever it holds a fully-formed time.
const display = ref(DISPLAY_RE.test(props.modelValue) ? props.modelValue : (props.modelValue || ""));

const commit = (text) => {
  if (text !== props.modelValue) emit("update:modelValue", text);
};

const onBlur = () => {
  const trimmed = String(display.value || "").trim();
  if (!trimmed) {
    commit("");
    return;
  }
  if (DISPLAY_RE.test(trimmed)) {
    commit(trimmed);
  } else {
    // Not a full, valid time — put back what was last accepted rather than leaving a half-typed value.
    display.value = props.modelValue || "";
  }
};

// The clock binds a 24-hour value derived from the display text; picking a time writes straight back
// through to modelValue (and the display box) in the 12-hour format the field stores.
const time24Model = computed({
  get: () => displayToTime24(display.value) || null,
  set: (val) => {
    const next = val ? time24ToDisplay(val) : "";
    display.value = next;
    commit(next);
  }
});

const clear = () => {
  display.value = "";
  commit("");
  popupRef.value?.hide();
};

const { text: ariaLabel } = useFieldLabel(toRef(props, "label"), toRef(props, "required"));
</script>

<style scoped>
.app-time__icon {
  color: var(--q-primary);
  cursor: pointer;
}
.app-time__icon--locked {
  color: #9aa5b1;
  cursor: default;
}
.app-time__clock {
  box-shadow: none;
  border-radius: 10px;
}
</style>
