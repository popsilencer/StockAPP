<template>
  <Toast position="top-right" />
  <div class="app-layout">
    <!-- Top Header -->
    <header class="top-header" v-if="auth.isLoggedIn">
      <div class="header-left">
        <button class="menu-toggle" @click="sidebarCollapsed = !sidebarCollapsed" title="Toggle menu">
          <i class="pi pi-bars"></i>
        </button>
        <h1>Popsilencer Stock APP</h1>
      </div>
      <div class="header-right">
        <button class="header-btn" @click="auth.logout" title="Logout">
          <i class="pi pi-sign-out"></i>
          <span>Logout</span>
        </button>
      </div>
    </header>

    <!-- Sidebar -->
    <aside class="sidebar" v-if="auth.isLoggedIn" :class="{ collapsed: sidebarCollapsed }">
      <nav class="sidebar-nav">
        <router-link to="/products" class="nav-item" active-class="active">
          <i class="pi pi-box"></i>
          <span>Products</span>
        </router-link>
        <router-link to="/movements" class="nav-item" active-class="active">
          <i class="pi pi-history"></i>
          <span>Movements</span>
        </router-link>
      </nav>
    </aside>

    <!-- Main Content -->
    <main class="main-content" :class="{ 'no-layout': !auth.isLoggedIn }">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from './stores/auth'

const auth = useAuthStore()
const sidebarCollapsed = ref(false)
</script>

<style>
/* ===== Pastel Pink Palette ===== */
:root {
  --pink-50: #fdf2f8;
  --pink-100: #fce7f3;
  --pink-200: #fbcfe8;
  --pink-300: #f9a8d4;
  --pink-400: #f472b6;
  --pink-500: #ec4899;
  --pink-600: #db2777;
  --pink-700: #be185d;
  --rose-50: #fff1f2;
  --rose-100: #ffe4e6;
  --rose-200: #fecdd3;
  --rose-400: #fb7185;
  --rose-500: #f43f5e;
  --rose-600: #e11d48;
  --purple-50: #faf5ff;
  --purple-100: #f3e8ff;
  --purple-400: #c084fc;
  --purple-500: #a855f7;
  --gray-50: #f9fafb;
  --gray-100: #f3f4f6;
  --gray-200: #e5e7eb;
  --gray-300: #d1d5db;
  --gray-400: #9ca3af;
  --gray-500: #6b7280;
  --gray-600: #4b5563;
  --gray-700: #374151;
  --gray-800: #1f2937;
  --gray-900: #111827;
  --green-50: #f0fdf4;
  --green-100: #dcfce7;
  --green-500: #22c55e;
  --green-600: #16a34a;
  --green-700: #15803d;
  --amber-50: #fffbeb;
  --amber-100: #fef3c7;
  --amber-400: #fbbf24;
  --amber-500: #f59e0b;
  --amber-600: #d97706;
  --blue-50: #eff6ff;
  --blue-400: #60a5fa;
  --blue-500: #3b82f6;
  --blue-600: #2563eb;
  --red-50: #fef2f2;
  --red-400: #f87171;
  --red-500: #ef4444;
  --red-600: #dc2626;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background: var(--gray-50);
  color: var(--gray-800);
  -webkit-font-smoothing: antialiased;
}

.app-layout {
  display: flex;
  min-height: 100vh;
}

/* ===== Top Header ===== */
.top-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  height: 60px;
  background: #fff;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 1.25rem;
  z-index: 200;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06), 0 1px 2px rgba(0, 0, 0, 0.04);
  border-bottom: 1px solid var(--pink-100);
}

.header-left {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.menu-toggle {
  width: 38px;
  height: 38px;
  border: none;
  border-radius: 10px;
  background: var(--pink-50);
  color: var(--pink-600);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  transition: all 0.2s;
}

.menu-toggle:hover {
  background: var(--pink-100);
  color: var(--pink-700);
}

.header-left h1 {
  font-size: 1.15rem;
  font-weight: 700;
  background: linear-gradient(135deg, var(--pink-500), var(--rose-500));
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  letter-spacing: -0.3px;
}

.header-right {
  display: flex;
  align-items: center;
}

.header-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 10px;
  background: var(--rose-50);
  color: var(--rose-600);
  cursor: pointer;
  font-size: 0.85rem;
  font-weight: 500;
  transition: all 0.2s;
}

.header-btn:hover {
  background: var(--rose-100);
  color: var(--rose-700);
}

.header-btn i {
  font-size: 0.95rem;
}

/* ===== Sidebar ===== */
.sidebar {
  position: fixed;
  top: 60px;
  left: 0;
  bottom: 0;
  width: 250px;
  background: #fff;
  border-right: 1px solid var(--gray-100);
  display: flex;
  flex-direction: column;
  z-index: 100;
  transition: width 0.3s ease, transform 0.3s ease;
  overflow: hidden;
}

.sidebar.collapsed {
  width: 68px;
}

.sidebar-nav {
  padding: 1rem 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  padding: 0.7rem 1rem;
  color: var(--gray-500);
  text-decoration: none;
  font-size: 0.9rem;
  font-weight: 500;
  border-radius: 10px;
  transition: all 0.2s;
  white-space: nowrap;
  overflow: hidden;
  position: relative;
}

.nav-item:hover {
  background: var(--pink-50);
  color: var(--pink-600);
}

.nav-item.active {
  background: linear-gradient(135deg, var(--pink-50), var(--rose-50));
  color: var(--pink-600);
  font-weight: 600;
}

.nav-item.active::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 3px;
  height: 50%;
  background: linear-gradient(180deg, var(--pink-400), var(--rose-400));
  border-radius: 0 3px 3px 0;
}

.nav-item i {
  font-size: 1.15rem;
  width: 24px;
  text-align: center;
  flex-shrink: 0;
}

.sidebar.collapsed .nav-item span {
  opacity: 0;
  width: 0;
}

/* ===== Main Content ===== */
.main-content {
  flex: 1;
  margin-top: 60px;
  margin-left: 250px;
  padding: 1.5rem;
  min-height: calc(100vh - 60px);
  transition: margin-left 0.3s ease;
}

.main-content.no-layout {
  margin-top: 0;
  margin-left: 0;
  padding: 0;
  min-height: 100vh;
}

.sidebar.collapsed ~ .main-content,
.app-layout:has(.sidebar.collapsed) .main-content {
  margin-left: 68px;
}

/* ===== Page Layout ===== */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.actions {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  flex-wrap: wrap;
}

/* ===== PrimeVue Overrides — Pastel Pink Theme ===== */

/* Buttons */
.p-button {
  border: none !important;
  border-radius: 10px !important;
  font-weight: 600 !important;
  font-size: 0.85rem !important;
  padding: 0.5rem 1rem !important;
  transition: all 0.2s !important;
  box-shadow: 0 1px 2px rgba(0,0,0,0.04) !important;
}

.p-button:active {
  transform: scale(0.97) !important;
}

/* Primary / default button */
.p-button:not(.p-button-success):not(.p-button-danger):not(.p-button-info):not(.p-button-warning):not(.p-button-secondary):not(.p-button-help):not(.p-button-text):not(.p-button-outlined) {
  background: linear-gradient(135deg, var(--pink-400), var(--pink-500)) !important;
  color: #fff !important;
}
.p-button:not(.p-button-success):not(.p-button-danger):not(.p-button-info):not(.p-button-warning):not(.p-button-secondary):not(.p-button-help):not(.p-button-text):not(.p-button-outlined):hover {
  background: linear-gradient(135deg, var(--pink-500), var(--pink-600)) !important;
}

/* Success button */
.p-button.p-button-success {
  background: linear-gradient(135deg, var(--green-500), #059669) !important;
  color: #fff !important;
}
.p-button.p-button-success:hover {
  background: linear-gradient(135deg, #059669, var(--green-700)) !important;
}

/* Danger button */
.p-button.p-button-danger {
  background: linear-gradient(135deg, var(--rose-400), var(--rose-500)) !important;
  color: #fff !important;
}
.p-button.p-button-danger:hover {
  background: linear-gradient(135deg, var(--rose-500), var(--rose-600)) !important;
}

/* Info button */
.p-button.p-button-info {
  background: linear-gradient(135deg, var(--blue-400), var(--blue-500)) !important;
  color: #fff !important;
}
.p-button.p-button-info:hover {
  background: linear-gradient(135deg, var(--blue-500), var(--blue-600)) !important;
}

/* Secondary button */
.p-button.p-button-secondary {
  background: var(--gray-100) !important;
  color: var(--gray-600) !important;
}
.p-button.p-button-secondary:hover {
  background: var(--gray-200) !important;
  color: var(--gray-700) !important;
}

/* Text buttons (icon buttons in table) */
.p-button.p-button-text {
  background: transparent !important;
  box-shadow: none !important;
  border-radius: 8px !important;
  width: 36px !important;
  height: 36px !important;
  padding: 0 !important;
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
}
.p-button.p-button-text:hover {
  background: var(--gray-100) !important;
}
.p-button.p-button-text.p-button-info { color: var(--blue-500) !important; }
.p-button.p-button-text.p-button-info:hover { background: var(--blue-50) !important; }
.p-button.p-button-text.p-button-success { color: var(--green-600) !important; }
.p-button.p-button-text.p-button-success:hover { background: var(--green-50) !important; }
.p-button.p-button-text.p-button-danger { color: var(--red-500) !important; }
.p-button.p-button-text.p-button-danger:hover { background: var(--red-50) !important; }
.p-button.p-button-text.p-button-secondary { color: var(--gray-500) !important; }
.p-button.p-button-text.p-button-secondary:hover { background: var(--gray-100) !important; }

/* Rounded text buttons */
.p-button.p-button-rounded {
  border-radius: 50% !important;
}

/* Datatable */
.p-datatable {
  border: none !important;
  border-radius: 12px !important;
  overflow: hidden !important;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06) !important;
}

.p-datatable .p-datatable-thead > tr > th {
  background: linear-gradient(135deg, var(--pink-50), var(--rose-50)) !important;
  color: var(--pink-700) !important;
  font-weight: 700 !important;
  font-size: 0.8rem !important;
  text-transform: uppercase !important;
  letter-spacing: 0.5px !important;
  border: none !important;
  padding: 0.85rem 1rem !important;
}

.p-datatable .p-datatable-tbody > tr {
  border-bottom: 1px solid var(--gray-100) !important;
  transition: background 0.15s !important;
}

.p-datatable .p-datatable-tbody > tr:last-child {
  border-bottom: none !important;
}

.p-datatable .p-datatable-tbody > tr:hover {
  background: var(--pink-50) !important;
}

.p-datatable .p-datatable-tbody > tr > td {
  padding: 0.75rem 1rem !important;
  font-size: 0.9rem !important;
  color: var(--gray-700) !important;
  border: none !important;
}

/* Dialog */
.p-dialog {
  border-radius: 16px !important;
  box-shadow: 0 20px 60px rgba(0,0,0,0.12) !important;
}

.p-dialog .p-dialog-header {
  background: linear-gradient(135deg, var(--pink-400), var(--pink-500)) !important;
  color: #fff !important;
  border-radius: 16px 16px 0 0 !important;
  padding: 1rem 1.25rem !important;
}

.p-dialog .p-dialog-header .p-dialog-title {
  color: #fff !important;
  font-weight: 700 !important;
  font-size: 1rem !important;
}

.p-dialog .p-dialog-header .p-dialog-header-icon {
  color: rgba(255,255,255,0.8) !important;
}
.p-dialog .p-dialog-header .p-dialog-header-icon:hover {
  color: #fff !important;
  background: rgba(255,255,255,0.2) !important;
  border-radius: 8px !important;
}

.p-dialog .p-dialog-content {
  padding: 1.25rem !important;
}

.p-dialog .p-dialog-footer {
  padding: 1rem 1.25rem !important;
  border-top: 1px solid var(--gray-100) !important;
  gap: 0.5rem !important;
}

/* Input */
.p-inputtext {
  border-radius: 10px !important;
  border: 1.5px solid var(--gray-200) !important;
  padding: 0.6rem 0.85rem !important;
  font-size: 0.9rem !important;
  transition: all 0.2s !important;
}

.p-inputtext:focus {
  border-color: var(--pink-400) !important;
  box-shadow: 0 0 0 3px rgba(236, 72, 153, 0.12) !important;
}

/* Textarea */
.p-textarea {
  border-radius: 10px !important;
  border: 1.5px solid var(--gray-200) !important;
}
.p-textarea:focus {
  border-color: var(--pink-400) !important;
  box-shadow: 0 0 0 3px rgba(236, 72, 153, 0.12) !important;
}

/* InputNumber */
.p-inputnumber .p-inputtext {
  border-radius: 10px !important;
  border: 1.5px solid var(--gray-200) !important;
}

/* Tag */
.p-tag {
  border-radius: 8px !important;
  font-weight: 600 !important;
  font-size: 0.8rem !important;
  padding: 0.25rem 0.6rem !important;
}

.p-tag.p-tag-success {
  background: var(--green-50) !important;
  color: var(--green-700) !important;
}

.p-tag.p-tag-danger {
  background: var(--rose-50) !important;
  color: var(--rose-600) !important;
}

/* Toast */
.p-toast .p-toast-message {
  border-radius: 12px !important;
  border-left-width: 4px !important;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08) !important;
}

.p-toast .p-toast-message.p-toast-message-success {
  background: #fff !important;
  border-left-color: var(--green-500) !important;
  color: var(--gray-700) !important;
}

.p-toast .p-toast-message.p-toast-message-error {
  background: #fff !important;
  border-left-color: var(--rose-500) !important;
  color: var(--gray-700) !important;
}

.p-toast .p-toast-message.p-toast-message-warn {
  background: #fff !important;
  border-left-color: var(--amber-500) !important;
  color: var(--gray-700) !important;
}

/* Column header sort icons */
.p-datatable .p-sortable-column-icon {
  color: var(--pink-400) !important;
}

/* Scrollbar */
::-webkit-scrollbar {
  width: 6px;
}
::-webkit-scrollbar-track {
  background: transparent;
}
::-webkit-scrollbar-thumb {
  background: var(--gray-200);
  border-radius: 3px;
}
::-webkit-scrollbar-thumb:hover {
  background: var(--gray-300);
}
</style>
