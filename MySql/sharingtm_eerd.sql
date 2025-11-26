-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema sharingtmdb
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema sharingtmdb
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `sharingtmdb` DEFAULT CHARACTER SET utf8 ;
USE `sharingtmdb` ;

-- -----------------------------------------------------
-- Table `sharingtmdb`.`User`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`User` (
  `userid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `username` VARCHAR(45) NOT NULL,
  `balance` INT NOT NULL DEFAULT 0,
  PRIMARY KEY (`userid`),
  UNIQUE INDEX `userid_UNIQUE` (`userid` ASC) VISIBLE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `sharingtmdb`.`RentalStation`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`RentalStation` (
  `rsid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `rs_name` VARCHAR(45) NOT NULL,
  `rs_address` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`rsid`),
  UNIQUE INDEX `rsid_UNIQUE` (`rsid` ASC) VISIBLE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `sharingtmdb`.`TimeMachine`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`TimeMachine` (
  `tmid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `model` VARCHAR(6) NOT NULL,
  `current_era_point` VARCHAR(50) NOT NULL,
  `rsid` INT UNSIGNED NULL,
  PRIMARY KEY (`tmid`),
  UNIQUE INDEX `tmid_UNIQUE` (`tmid` ASC) VISIBLE,
  INDEX `fk_TimeMachine_RentalStation1_idx` (`rsid` ASC) VISIBLE,
  CONSTRAINT `fk_TimeMachine_RentalStation1`
    FOREIGN KEY (`rsid`)
    REFERENCES `sharingtmdb`.`RentalStation` (`rsid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `sharingtmdb`.`Ticket`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`Ticket` (
  `ticketid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `purchase_time` DATETIME NOT NULL,
  `expire_time` DATETIME NOT NULL,
  `status` VARCHAR(45) NOT NULL,
  `userid` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`ticketid`),
  UNIQUE INDEX `ticketid_UNIQUE` (`ticketid` ASC) VISIBLE,
  INDEX `fk_Ticket_User_idx` (`userid` ASC) VISIBLE,
  CONSTRAINT `fk_Ticket_User`
    FOREIGN KEY (`userid`)
    REFERENCES `sharingtmdb`.`User` (`userid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `sharingtmdb`.`Rental`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`Rental` (
  `rentalid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `rental_time` DATETIME NOT NULL,
  `return_time` DATETIME NULL,
  `overdue_minutes` INT NOT NULL DEFAULT 0,
  `overdue_fee` INT NOT NULL DEFAULT 0,
  `userid` INT UNSIGNED NOT NULL,
  `tmid` INT UNSIGNED NOT NULL,
  `depart_rs` INT UNSIGNED NOT NULL,
  `arrive_rs` INT UNSIGNED NULL,
  PRIMARY KEY (`rentalid`),
  UNIQUE INDEX `rentalid_UNIQUE` (`rentalid` ASC) VISIBLE,
  INDEX `fk_Rental_User1_idx` (`userid` ASC) VISIBLE,
  INDEX `fk_Rental_TimeMachine1_idx` (`tmid` ASC) VISIBLE,
  INDEX `fk_Rental_RentalStation1_idx` (`depart_rs` ASC) VISIBLE,
  INDEX `fk_Rental_RentalStation2_idx` (`arrive_rs` ASC) VISIBLE,
  CONSTRAINT `fk_Rental_User1`
    FOREIGN KEY (`userid`)
    REFERENCES `sharingtmdb`.`User` (`userid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Rental_TimeMachine1`
    FOREIGN KEY (`tmid`)
    REFERENCES `sharingtmdb`.`TimeMachine` (`tmid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Rental_RentalStation1`
    FOREIGN KEY (`depart_rs`)
    REFERENCES `sharingtmdb`.`RentalStation` (`rsid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Rental_RentalStation2`
    FOREIGN KEY (`arrive_rs`)
    REFERENCES `sharingtmdb`.`RentalStation` (`rsid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
