import { createApiClient } from '../../shared/apiClient';

export interface HomeLesson {
  id: number;
  title: string;
  content: string;
  order: number;
  completionPercent: number;
  isCompleted: boolean;
  lastStudiedAt?: string | null;
  reactionIllustrations: LessonReactionIllustration[];
}

export interface LessonReactionIllustration {
  equation: string;
  isHeated: boolean;
  note?: string | null;
}

export interface HomeChapter {
  id: number;
  title: string;
  description?: string | null;
  order: number;
  lessons: HomeLesson[];
}

export interface LessonProgress {
  lessonId: number;
  completionPercent: number;
  isCompleted: boolean;
  lastStudiedAt?: string | null;
}

export interface AboutContent {
  id: number;
  title: string;
  content: string;
  imageUrl?: string | null;
  order: number;
}

export interface LookupElement {
  id: number;
  atomicNumber: number;
  symbol: string;
  name: string;
  englishName?: string | null;
  atomicMass?: number | null;
  period?: number | null;
  group?: number | null;
  block?: string | null;
  type?: string | null;
  state?: string | null;
  color?: string | null;
  electronegativity?: number | null;
  electronConfiguration?: string | null;
  description?: string | null;
  applications?: string | null;
}

export interface LookupChemical {
  id: number;
  name: string;
  formula: string;
  type?: string | null;
  molarMass?: number | null;
  state?: string | null;
  color?: string | null;
  defaultPh?: number | null;
  defaultConcentration?: number | null;
  solubility?: string | null;
  properties?: string | null;
  applications?: string | null;
  description?: string | null;
  phenomenon?: string | null;
}

const apiClient = createApiClient();

export async function getChapters() {
  return (await apiClient.get<HomeChapter[]>('/api/learning/chapters')).data;
}

export async function getLearningProgress() {
  return (await apiClient.get<LessonProgress[]>('/api/learning/progress')).data;
}

export async function saveLessonProgress(lessonId: number, completionPercent: number, isCompleted = false) {
  return (await apiClient.put<LessonProgress>(`/api/learning/lessons/${lessonId}/progress`, {
    completionPercent,
    isCompleted,
  })).data;
}

export async function getAboutContent() {
  return (await apiClient.get<AboutContent[]>('/api/about')).data;
}

export async function getLookupElements() {
  return (await apiClient.get<LookupElement[]>('/api/lookup/elements')).data;
}

export async function getLookupChemicals() {
  return (await apiClient.get<LookupChemical[]>('/api/lookup/chemicals')).data;
}
