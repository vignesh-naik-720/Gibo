/**
 * Zustand State Store for Daily Plan & Offline Tasks
 * Path: src/mobile/src/features/dailyPlan/state/useDailyPlanStore.ts
 */

import { create } from 'zustand';
import { DailyPlanDto, TaskInstanceDto, EvidenceSource } from '../contracts/dailyPlan.types';
import { dailyPlanApi } from '../services/dailyPlanApiService';

interface DailyPlanState {
  plan: DailyPlanDto | null;
  isLoading: boolean;
  error: string | null;
  totalXp: number;

  fetchTodayPlan: () => Promise<void>;
  generateNewPlan: (locale?: string) => Promise<void>;
  completeTaskLocally: (taskId: string, source: EvidenceSource) => void;
  swapTaskLocally: (taskId: string, replacement: TaskInstanceDto) => void;
  adjustTargetLocally: (taskId: string, newTarget: number) => void;
}

export const useDailyPlanStore = create<DailyPlanState>((set, get) => ({
  plan: null,
  isLoading: false,
  error: null,
  totalXp: 0,

  fetchTodayPlan: async () => {
    set({ isLoading: true, error: null });
    try {
      const plan = await dailyPlanApi.getTodayPlan();
      set({ plan, totalXp: plan.totalXpEarned, isLoading: false });
    } catch (err: any) {
      console.warn('Could not fetch active plan online, falling back to local generation');
      // If no plan exists, generate one
      await get().generateNewPlan();
    }
  },

  generateNewPlan: async (locale = 'en') => {
    set({ isLoading: true, error: null });
    try {
      const plan = await dailyPlanApi.generatePlan(false, locale);
      set({ plan, totalXp: plan.totalXpEarned, isLoading: false });
    } catch (err: any) {
      // Offline fallback: Generate local sample plan
      const fallbackPlan: DailyPlanDto = {
        planId: 'local-plan-' + Date.now(),
        planDate: new Date().toISOString().split('T')[0],
        algorithmVersion: 'v2.1-offline',
        userEditsApplied: false,
        totalXpEarned: 0,
        tasks: [
          {
            taskInstanceId: 'task-1',
            taskTemplateId: '10000000-0000-0000-0000-000000000001',
            domain: 'Movement',
            actionVerb: 'Walk',
            title: locale === 'ar' ? 'مشي لمدة 10 دقائق داخل مركز تسوق مكيّف' : '10-Minute Air-Conditioned Mall Walk',
            targetValue: 10,
            targetUnit: 'mins',
            estimatedMinutes: 10,
            state: 'Scheduled',
            evidenceSource: null,
            xpAwarded: 0,
            rationale: locale === 'ar' ? 'مثالي لتجنب حرارة النهار والحفاظ على النشاط البدني.' : 'Perfect for avoiding daytime heat while maintaining active mobility.',
            isStarterFallback: true,
            displayOrder: 1,
          },
          {
            taskInstanceId: 'task-2',
            taskTemplateId: '30000000-0000-0000-0000-000000000001',
            domain: 'Nutrition',
            actionVerb: 'Hydrate',
            title: locale === 'ar' ? 'كوب ماء كبير (500 مل) فور الاستيقاظ صباحاً' : 'Morning 500ml Hydration Kickstart',
            targetValue: 1,
            targetUnit: 'glass (500ml)',
            estimatedMinutes: 2,
            state: 'Scheduled',
            evidenceSource: null,
            xpAwarded: 0,
            rationale: locale === 'ar' ? 'يعوض السوائل المفقودة ليلاً بسبب هواء التكييف الجاف في المنزل.' : 'Replenishes fluids lost overnight under continuous home air-conditioning.',
            isStarterFallback: true,
            displayOrder: 2,
          }
        ]
      };
      set({ plan: fallbackPlan, isLoading: false });
    }
  },

  completeTaskLocally: (taskId: string, source: EvidenceSource) => {
    const currentPlan = get().plan;
    if (!currentPlan) return;

    const updatedTasks = currentPlan.tasks.map((task) => {
      if (task.taskInstanceId === taskId && task.state !== 'Completed') {
        return {
          ...task,
          state: 'Completed' as const,
          evidenceSource: source,
          xpAwarded: 10,
        };
      }
      return task;
    });

    const newXp = updatedTasks.reduce((sum, t) => sum + t.xpAwarded, 0);

    set({
      plan: {
        ...currentPlan,
        tasks: updatedTasks,
        totalXpEarned: newXp,
      },
      totalXp: newXp,
    });
  },

  swapTaskLocally: (taskId: string, replacement: TaskInstanceDto) => {
    const currentPlan = get().plan;
    if (!currentPlan) return;

    const updatedTasks = currentPlan.tasks.map((t) =>
      t.taskInstanceId === taskId ? replacement : t
    );

    set({
      plan: {
        ...currentPlan,
        tasks: updatedTasks,
        userEditsApplied: true,
      },
    });
  },

  adjustTargetLocally: (taskId: string, newTarget: number) => {
    const currentPlan = get().plan;
    if (!currentPlan) return;

    const updatedTasks = currentPlan.tasks.map((t) =>
      t.taskInstanceId === taskId ? { ...t, targetValue: newTarget } : t
    );

    set({
      plan: {
        ...currentPlan,
        tasks: updatedTasks,
        userEditsApplied: true,
      },
    });
  },
}));
