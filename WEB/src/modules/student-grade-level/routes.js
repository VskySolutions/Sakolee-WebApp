export default [
  {
    path: "/student-grade-levels",
    component: () => import("layouts/layout.vue"),
    children: [
      // Route for listing all student grade levels (Index Page)
      {
        path: "",
        name: "student_grade_levels",
        component: () => import("modules/student-grade-level/pages/index.vue"),
        meta: { requiresAuth: true, title: "Student Grade Levels" }
      },
      // Route for creating a new student grade level record
      // {
      //   path: "create",
      //   name: "student_grade_level_create",
      //   component: () => import("modules/student-grade-level/components/create_edit.vue"),
      //   meta: { requiresAuth: true, title: "Create Student Grade Level" }
      // },
      // // Route for viewing detailed information of a specific student grade level record
      // {
      //   path: ":id",
      //   name: "student_grade_level_detail",
      //   component: () => import("modules/student-grade-level/components/view.vue"),
      //   meta: { requiresAuth: true, title: "Student Grade Level Details" }
      // }
    ]
  }
];