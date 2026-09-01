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
