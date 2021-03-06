#ifndef IMAGE_CONVERTER_HPP
#define IMAGE_CONVERTER_HPP

#include <opencv2/opencv.hpp>
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/highgui/highgui_c.h>
#include <QImage>

cv::Mat qimage_to_mat_ref(QImage &img, int format)
{
  return cv::Mat(img.height(), img.width(),
                 format, img.bits(), img.bytesPerLine());
}

cv::Mat qimage_to_mat_cpy(QImage const &img, int format)
{
  return cv::Mat(img.height(), img.width(), format,
                 const_cast<uchar*>(img.bits()),
                 img.bytesPerLine()).clone();
}


QImage mat_to_qimage_cpy(cv::Mat const &mat,
                         QImage::Format format)
{
  return QImage(mat.data, mat.cols, mat.rows,
                mat.step, format).copy();
}

void qimage_to_mat(const QImage& image, cv::OutputArray out) {

  switch(image.format()) {
  case QImage::Format_Invalid:
  {
    cv::Mat empty;
    empty.copyTo(out);
    break;
  }
  case QImage::Format_RGB32:
  {
    cv::Mat view(image.height(),image.width(),CV_8UC4,(void *)image.constBits(),image.bytesPerLine());
    view.copyTo(out);
    break;
  }
  case QImage::Format_RGB888:
  {
    cv::Mat view(image.height(),image.width(),CV_8UC3,(void *)image.constBits(),image.bytesPerLine());
    cvtColor(view, out, cv::COLOR_RGB2BGR);
    break;
  }
  default:
  {
    QImage conv = image.convertToFormat(QImage::Format_ARGB32);
    cv::Mat view(conv.height(),conv.width(),CV_8UC4,(void *)conv.constBits(),conv.bytesPerLine());
    view.copyTo(out);
    break;
  }
  }
}

void mat_to_qimage(cv::InputArray image, QImage& out)
{
  switch(image.type())
  {
  case CV_8UC4:
  {
    cv::Mat view(image.getMat());
    QImage view2(view.data, view.cols, view.rows, view.step[0], QImage::Format_ARGB32);
    out = view2.copy();
    break;
  }
  case CV_8UC3:
  {
    cv::Mat mat;
    cvtColor(image, mat, cv::COLOR_BGR2BGRA); //COLOR_BGR2RGB doesn't behave so use RGBA
    QImage view(mat.data, mat.cols, mat.rows, mat.step[0], QImage::Format_ARGB32);
    out = view.copy();
    break;
  }
  case CV_8UC1:
  {
    cv::Mat mat;
    cvtColor(image, mat, cv::COLOR_GRAY2BGRA);
    QImage view(mat.data, mat.cols, mat.rows, mat.step[0], QImage::Format_ARGB32);
    out = view.copy();
    break;
  }
  default:
  {
    //throw invalid_argument("Image format not supported");
    break;
  }
  }
}



#endif // IMAGE_CONVERTER_HPP
