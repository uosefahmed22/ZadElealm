import { CourseDto } from '../catalog/catalog.models';

export interface QuizChoiceDto {
  id: number;
  text: string;
}

export interface QuizQuestionDto {
  id: number;
  text: string;
  quizId: number;
  choices: QuizChoiceDto[];
}

export interface QuizDto {
  id: number;
  name: string;
  description: string;
  createdAt: string;
  course: CourseDto;
  questions: QuizQuestionDto[];
}

export interface StudentAnswerDto {
  questionId: number;
  choiceId: number;
}

export interface QuizSubmissionDto {
  quizId: number;
  studentAnswers: StudentAnswerDto[];
}

export interface QuestionResultDto {
  questionId: number;
  questionText: string;
  isCorrect: boolean;
  selectedChoice: number;
  correctChoice: number;
}

export interface QuizResultDto {
  quizName: string;
  score: number;
  isCompleted: boolean;
  totalQuestions: number;
  correctAnswers: number;
  unansweredQuestions: number;
  date: string;
  questionResults: QuestionResultDto[];
}

export interface CertificateDto {
  id: number;
  name: string;
  description: string;
  pdfUrl: string;
  completedDate: string;
  userName: string;
  quizName: string;
}

export interface AssessmentSummaryDto {
  id: number;
  name: string;
  description: string;
  categoryName: string;
  passingScore: number;
  isEligible: boolean;
  isCompleted: boolean;
  bestScore: number | null;
}

export interface AssessmentQuestionDto {
  id: number;
  text: string;
  choices: QuizChoiceDto[];
}

export interface CategoryAssessmentDto {
  id: number;
  name: string;
  description: string;
  passingScore: number;
  questions: AssessmentQuestionDto[];
}

export interface AssessmentSubmissionDto {
  studentAnswers: StudentAnswerDto[];
}

export interface AssessmentResultDto {
  assessmentName: string;
  score: number;
  isCompleted: boolean;
  totalQuestions: number;
  correctAnswers: number;
  date: string;
}
