<template>
  <q-card flat bordered class="panel">
    <q-card-section class="row items-start no-wrap q-pb-sm">
      <div class="col">
        <div class="panel__title">My Tasks</div>
        <div class="panel__caption">Assigned tasks</div>
      </div>
      <q-btn unelevated no-caps dense size="sm" color="primary" class="panel__add" icon="o_add" label="Add Task" @click="$emit('add')" />
    </q-card-section>

    <q-card-section class="q-pt-none column q-gutter-sm">
      <div v-if="!rows.length" class="text-grey-6 text-center q-pa-md">Nothing assigned to you.</div>

      <div v-for="task in rows" :key="task.id" class="task-row column">
        <div class="row items-center no-wrap">
          <div class="col task-row__title">{{ task.title }}</div>
          <span class="task-row__priority" :style="priorityStyle(task.priority)">
            {{ task.priority }}
          </span>
        </div>
        <div class="task-row__meta">{{ metaFor(task) }}</div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  // [{ id, title, due, owner, priority }]
  tasks: { type: Array, default: () => [] }
});

defineEmits(["add"]);

const rows = computed(() => props.tasks || []);

// The prototype badges priority rather than just colouring the text: red-50/error for urgent,
// amber-50/amber-600 for medium.
const PRIORITY_TONES = {
  urgent: { color: "var(--error)", background: "var(--error-tint)" },
  medium: { color: "var(--warning)", background: "var(--warning-tint)" },
  low: { color: "var(--outline)", background: "var(--surface-container)" }
};

const priorityStyle = (priority) =>
  PRIORITY_TONES[String(priority || "").toLowerCase()] ?? PRIORITY_TONES.low;

// "Today • Sarah J." — the separator is dropped when either half is missing.
const metaFor = (task) => [task.due, task.owner].filter(Boolean).join(" • ");
</script>

<style scoped>
.panel { border-radius: 16px; height: 100%; }

.panel__title {
  font-size: 15px;
  font-weight: 700;
  color: var(--on-surface);
}

.panel__caption {
  font-size: 12px;
  color: var(--outline);
}

.panel__add { font-weight: 700; border-radius: 8px; }

.task-row {
  background: var(--surface-container-low);
  border: 1px solid var(--line);
  border-radius: 12px;
  padding: 10px 12px;
}

.task-row__title {
  font-size: 12px;
  font-weight: 600;
  color: var(--on-surface);
}

.task-row__meta {
  font-size: 11px;
  color: var(--outline);
  margin-top: 2px;
}

.task-row__priority {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  border-radius: 4px;
  padding: 1px 6px;
  margin-left: 8px;
  white-space: nowrap;
}
</style>
