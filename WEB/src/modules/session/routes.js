export default [
  {
    path: "/sessions",
    component: () => import("layouts/layout.vue"),
    children: [
      // Route for listing all sessions (Index Page)
      {
        path: "",
        name: "sessions",
        component: () => import("modules/session/pages/index.vue"),
        meta: { requiresAuth: true, permissions: ["classSessions.read"], title: "Sessions" }
      },
      // Route for creating a new session record
      {
        path: "create",
        name: "session_create",
        component: () => import("modules/session/components/create_edit_session.vue"),
        meta: { requiresAuth: true, permissions: ["classSessions.write"], title: "Create Session" }
      },
    //   // Route for editing an existing session record by its unique identifier
    //   {
    //     path: ":id/edit",
    //     name: "session_edit",
    //     component: () => import("modules/session/components/create_edit_session.vue"),
    //     meta: { requiresAuth: true, permissions: ["classSessions.write"], title: "Edit Session" }
    //   },
      // Route for viewing detailed information of a specific session record
      {
        path: ":id",
        name: "session_detail",
        component: () => import("modules/session/components/view_session.vue"),
        meta: { requiresAuth: true, permissions: ["classSessions.read"], title: "Session Details" }
      }
    ]
  }
];